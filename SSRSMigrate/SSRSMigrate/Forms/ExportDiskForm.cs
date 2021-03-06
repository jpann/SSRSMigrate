﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SSRSMigrate.Exporter;
using SSRSMigrate.SSRS.Item;
using SSRSMigrate.SSRS.Reader;
using SSRSMigrate.Status;
using SSRSMigrate.Utility;
using Ninject.Extensions.Logging;

namespace SSRSMigrate.Forms
{
    public partial class ExportDiskForm : Form
    {
        private readonly IReportServerReader mReportServerReader = null;
        private readonly FolderItemExporter mFolderExporter = null;
        private readonly ReportItemExporter mReportExporter = null;
        private readonly DataSourceItemExporter mDataSourceExporter = null;
        private readonly ILoggerFactory mLoggerFactory = null;

        private readonly string mSourceRootPath = null;
        private readonly string mExportDestinationPath = null;

        private BackgroundWorker mSourceRefreshWorker = null;
        private BackgroundWorker mExportWorker = null;
        private ILogger mLogger = null;

        private DebugForm mDebugForm = null;
        private SummaryForm mSummaryForm = null;

        #region Properties
        public DebugForm DebugForm
        {
            set { this.mDebugForm = value; }
        }
        #endregion

        public ExportDiskForm(string sourceRootPath,
            string destinationPath,
            IReportServerReader reader,
            FolderItemExporter folderExporter,
            ReportItemExporter reportExporter,
            DataSourceItemExporter dataSourceExporter,
            ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrEmpty(sourceRootPath))
                throw new ArgumentException("sourceRootPath");

            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("destinationPath");

            if (reader == null)
                throw new ArgumentNullException("reader");

            if (folderExporter == null)
                throw new ArgumentNullException("folderExporter");

            if (reportExporter == null)
                throw new ArgumentNullException("reportExporter");

            if (dataSourceExporter == null)
                throw new ArgumentNullException("dataSourceExporter");

            if (loggerFactory == null)
                throw new ArgumentNullException("loggerFactory");

            InitializeComponent();

            this.mSourceRootPath = sourceRootPath;
            this.mExportDestinationPath = destinationPath;
            this.mReportServerReader = reader;
            this.mFolderExporter = folderExporter;
            this.mReportExporter = reportExporter;
            this.mDataSourceExporter = dataSourceExporter;
            this.mLoggerFactory = loggerFactory;

            this.mLogger = mLoggerFactory.GetCurrentClassLogger();     
            this.mSummaryForm = new SummaryForm();
        }

        #region UI Events
        private void ExportDiskForm_Load(object sender, EventArgs e)
        {
            // Create BackgroundWorker that is used to read items from ReportServer
            this.mSourceRefreshWorker = new BackgroundWorker();
            this.mSourceRefreshWorker.WorkerReportsProgress = true;
            this.mSourceRefreshWorker.WorkerSupportsCancellation = true;
            this.mSourceRefreshWorker.DoWork += new DoWorkEventHandler(this.SourceRefreshReportsWorker);
            this.mSourceRefreshWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bw_SourceRefreshReportsCompleted);
            this.mSourceRefreshWorker.ProgressChanged += new ProgressChangedEventHandler(this.bw_SourceRefreshReportsProgressChanged);
        
            // Create BackgroundWorker that is used to export items to disk
            this.mExportWorker = new BackgroundWorker();
            this.mExportWorker.WorkerReportsProgress = true;
            this.mExportWorker.WorkerSupportsCancellation = true;
            this.mExportWorker.DoWork += new DoWorkEventHandler(this.ExportItemsWorker);
            this.mExportWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bw_ExportItems_Completed);
            this.mExportWorker.ProgressChanged += new ProgressChangedEventHandler(this.bw_ExportItems_ProgressChanged);
        
        }

        private void ExportDiskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If a report refresh is in progress, don't allow the form to close
            if (this.mSourceRefreshWorker != null)
                if (this.mSourceRefreshWorker.IsBusy)
                    e.Cancel = true;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // If there are no items in the list, there is nothing to export
            if (this.lstSrcReports.Items.Count <= 0)
                return;

            this.ExportToDisk();
        }

        private void btnSrcRefreshReports_Click(object sender, EventArgs e)
        {
            if (this.mSourceRefreshWorker.IsBusy)
            {
                MessageBox.Show("Refresh in progress. Please wait for it to finish.",
                    "Refresh In Progress",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);

                return;
            }

            this.SourceRefreshReports();
        }
        
        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.lstSrcReports.Items.OfType<ListViewItem>().ToList().ForEach(item => item.Checked = true);
        }

        private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.lstSrcReports.Items.OfType<ListViewItem>().ToList().ForEach(item => item.Checked = false);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            if (this.mDebugForm.Visible)
                this.mDebugForm.Hide();
            else
                this.mDebugForm.Show();
        }
        #endregion

        #region Source Server Reports Methods
        private void SourceRefreshReports()
        {
            this.lstSrcReports.Items.Clear();

            try
            {
                this.btnExport.Enabled = false;
                this.btnSrcRefreshReports.Enabled = false;
                this.mSourceRefreshWorker.RunWorkerAsync(this.mSourceRootPath);
            }
            catch (Exception er)
            {
                string msg = string.Format("Error getting list of items from '{0}'.",
                    this.mSourceRootPath);

                this.mDebugForm.LogMessage(msg, er);

                this.mLogger.Error(er, msg);
                MessageBox.Show(string.Format("Error refreshing items at '{0}':\n\r{1}", this.mSourceRootPath,
                    er.Message),
                    "Refresh Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void SourceRefreshReportsWorker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string path = (string)e.Argument;

            // Get folders from the specified path and add them to the Reports ListView control
            this.mReportServerReader.GetFolders(path, ReportsReader_FolderReporter);

            // Get data sources from the specified path and add them to the Reports ListView control
            this.mReportServerReader.GetDataSources(path, ReportsReader_DataSourceReporter);

            // Get reports from the specified path and add them to the Reports ListView control
            this.mReportServerReader.GetReports(path, ReportsReader_ReportReporter);
        }

        private void bw_SourceRefreshReportsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string msg = null;

            if ((e.Cancelled == true))
            {
                msg = string.Format("Cancelled. {0}", e.Result);

                this.mDebugForm.LogMessage(msg);
            }
            else if ((e.Error != null))
            {
                msg = string.Format("Error. {0}", e.Error);

                this.mLogger.Error(e.Error, "Error during item refresh");

                this.mDebugForm.LogMessage(msg, e.Error);
                MessageBox.Show(msg,
                    "Refresh Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                msg = string.Format("Completed.");

                // Only allow exporting if the refresh completed without error 
                //  and there are items to export.
                if (this.lstSrcReports.Items.Count > 0)
                    this.btnExport.Enabled = true;

                this.mDebugForm.LogMessage(msg);
            }

            this.mLogger.Info("Item refresh: {0}", msg);
            this.btnSrcRefreshReports.Enabled = true;
            this.lblStatus.Text = msg;
        }

        private void bw_SourceRefreshReportsProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ReportItem report = (ReportItem)e.UserState;

            progressBar.Value = e.ProgressPercentage;
            progressBar.Maximum = 100;
            progressBar.ToolTipText = report.Name;
        }
        
        private void ReportsReader_FolderReporter(ReportServerItem item)
        {
            if (item == null)
            {
                this.mLogger.Warn("ReportsReader_FolderReporter - item contains a NULL value.");

                return;
            }

            ListViewItem oItem = new ListViewItem(item.Name);
            oItem.Checked = true;
            //oItem.Tag = item;
            oItem.Tag = item.Path;
            oItem.SubItems.Add(item.Path);

            // Assign to proper ListViewGroup
            oItem.Group = this.lstSrcReports.Groups["foldersGroup"];

            this.lstSrcReports.Invoke(new Action(() => this.lstSrcReports.Items.Add(oItem)));
            this.lstSrcReports.Invoke(new Action(() => oItem.EnsureVisible()));

            this.lblStatus.Text = string.Format("Refreshing item '{0}'...", item.Path);

            this.mLogger.Debug("Refreshing item '{0}'...", item.Path);

            this.mDebugForm.LogMessage(string.Format("Refreshing item '{0}'...", item.Path));
        
        }

        private void ReportsReader_DataSourceReporter(ReportServerItem item)
        {
            if (item == null)
            {
                this.mLogger.Warn("ReportsReader_DataSourceReporter - item contains a NULL value.");

                return;
            }

            ListViewItem oItem = new ListViewItem(item.Name);
            oItem.Checked = true;
            //oItem.Tag = item;
            oItem.Tag = item.Path;
            oItem.SubItems.Add(item.Path);

            // Assign to proper ListViewGroup
            oItem.Group = this.lstSrcReports.Groups["dataSourcesGroup"];

            this.lstSrcReports.Invoke(new Action(() => this.lstSrcReports.Items.Add(oItem)));
            this.lstSrcReports.Invoke(new Action(() => oItem.EnsureVisible()));

            this.lblStatus.Text = string.Format("Refreshing item '{0}'...", item.Path);

            this.mLogger.Debug("Refreshing item '{0}'...", item.Path);

            this.mDebugForm.LogMessage(string.Format("Refreshing item '{0}'...", item.Path));
        
        }

        private void ReportsReader_ReportReporter(ReportServerItem item)
        {
            if (item == null)
            {
                this.mLogger.Warn("ReportsReader_ReportReporter - item contains a NULL value.");

                return;
            }

            ListViewItem oItem = new ListViewItem(item.Name);
            oItem.Checked = true;
            //oItem.Tag = item;
            oItem.Tag = item.Path;
            oItem.SubItems.Add(item.Path);

            // Assign to proper ListViewGroup
            oItem.Group = this.lstSrcReports.Groups["reportsGroup"];

            this.lstSrcReports.Invoke(new Action(() => this.lstSrcReports.Items.Add(oItem)));
            this.lstSrcReports.Invoke(new Action(() => oItem.EnsureVisible()));

            this.lblStatus.Text = string.Format("Refreshing item '{0}'...", item.Path);

            this.mLogger.Debug("Refreshing item '{0}'...", item.Path);

            this.mDebugForm.LogMessage(string.Format("Refreshing item '{0}'...", item.Path));
        }
        #endregion

        #region Export Methods
        private void ExportToDisk()
        {
            try
            {
                this.btnExport.Enabled = false;
                this.btnSrcRefreshReports.Enabled = false;
                this.mExportWorker.RunWorkerAsync(this.mExportDestinationPath);
            }
            catch (Exception er)
            {
                this.mLogger.Fatal(er, "Error exporting items.");

                MessageBox.Show(
                    string.Format("Error exporting items to '{0}':\n\r{1}", 
                    this.mExportDestinationPath, er.Message),
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Used for getting the ListView items from within the BackgroundWorker thread.
        private delegate ListView.ListViewItemCollection GetItems(ListView listView);
        
        // Used for getting the ListView items from within the BackgroundWorker thread.
        private ListView.ListViewItemCollection GetListViewItems(ListView listView)
        {
            ListView.ListViewItemCollection tmpListViewColl = new ListView.ListViewItemCollection(new ListView());

            if (!listView.InvokeRequired)
            {
                foreach (ListViewItem item in listView.Items)
                    tmpListViewColl.Add((ListViewItem)item.Clone());

                return tmpListViewColl;
            }
            else
                return (ListView.ListViewItemCollection)this.Invoke(new GetItems(GetListViewItems), new object[] { listView });
        }

        private void ExportItemsWorker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string exportPath = (string)e.Argument;

            // Stopwatch to track how long the export takes
            Stopwatch watch = new Stopwatch();

            // Start stopwatch to get how long it takes to get the total number of checked items
            watch.Start();

            IEnumerable<ListViewItem> lvItems = GetListViewItems(this.lstSrcReports).Cast<ListViewItem>();

            // Get total count of items in ListView that are checked
            int totalItems = lvItems.Where(lv => lv.Checked == true).Count();
            int progressCounter = 0;
            int reportsExportedCounter = 0;
            int reportsTotalCount = 0;
            int foldersExportedCounter = 0;
            int foldersTotalCount = 0;
            int dataSourcesExportedCounter = 0;
            int dataSourcesTotalCount = 0;

            // Stop stopwatch after getting the total number of checked items, and log how long it took
            watch.Stop();
            this.mLogger.Debug("GetListViewItems - Took {0} seconds to get checked ListView items", watch.Elapsed.TotalSeconds);

            // Start stopwatch to get how long it takes to export everything
            watch.Start();

            // Export folders
            // Get path of ListView items in the folder group that are checked.
            var folderPaths = from lv in lvItems
                              where lv.Group.Name == "foldersGroup" &&
                              lv.Checked == true
                              select (string)lv.Tag;

            foldersTotalCount = folderPaths.Count();

            foreach (string folderPath in folderPaths)
            {
                ExportStatus status = null;

                if (!string.IsNullOrEmpty(folderPath))
                {
                    FolderItem folderItem = this.mReportServerReader.GetFolder(folderPath);

                    if (folderItem != null)
                    {
                        // Path to where to export folder on disk
                        string saveFilePath = exportPath + SSRSUtil.GetServerPathToPhysicalPath(folderPath);

                        status = this.mFolderExporter.SaveItem(folderItem,
                            saveFilePath,
                            true);

                        this.mLogger.Debug("ExportItemsWorker - FolderItem.Success = {0}; FromPath = {1}; ToPath = {2}",
                            status.Success,
                            status.FromPath,
                            status.ToPath);
                       
                        if (status.Success)
                            ++foldersExportedCounter;
                    }
                    else
                        this.mLogger.Warn("ExportItemsWorker - FolderItem for path '{0}' returned NULL.", folderPath);
                }

                // Always report progress, even if a ListViewItem has an empty path and even if the item isn't retrieved by ReportServerReader.
                // This will keep the progress bar value from suddenly jumping up several values.
                if (worker != null)
                    worker.ReportProgress(((++progressCounter * 100) / totalItems), status);
                else
                {
                    this.mLogger.Warn("ExportItemsWorker - worker is NULL.");
                }
            }

            // Export data sources
            var dataSourcePaths = from lv in lvItems
                                  where lv.Group.Name == "dataSourcesGroup" &&
                                  lv.Checked == true
                                  select (string)lv.Tag;

            dataSourcesTotalCount = dataSourcePaths.Count();

            foreach (string dataSourcePath in dataSourcePaths)
            {
                ExportStatus status = null;

                if (!string.IsNullOrEmpty(dataSourcePath))
                {
                    DataSourceItem dataSourceItem = this.mReportServerReader.GetDataSource(dataSourcePath);

                    if (dataSourceItem != null)
                    {
                        // Path to where to export data source on disk
                        string saveFilePath = exportPath + SSRSUtil.GetServerPathToPhysicalPath(dataSourcePath, "json");

                        status = this.mDataSourceExporter.SaveItem(dataSourceItem,
                            saveFilePath,
                            true);

                        this.mLogger.Debug("ExportItemsWorker - DataSourceItem.Success = {0}; FromPath = {1}; ToPath = {2}",
                            status.Success,
                            status.FromPath,
                            status.ToPath);

                        if (status.Success)
                            ++dataSourcesExportedCounter;
                    }
                    else
                        this.mLogger.Warn("ExportItemsWorker - DataSourceItem for path '{0}' returned NULL.", dataSourcePath);
                }

                // Always report progress, even if a ListViewItem has an empty path and even if the item isn't retrieved by ReportServerReader.
                // This will keep the progress bar value from suddenly jumping up several values.
                if (worker != null)
                    worker.ReportProgress(((++progressCounter * 100) / totalItems), status);
                else
                {
                    this.mLogger.Warn("ExportItemsWorker - worker is NULL.");
                }
            }

            // Export reports
            var reportPaths = from lv in lvItems
                                  where lv.Group.Name == "reportsGroup" &&
                                  lv.Checked == true
                                  select (string)lv.Tag;

            reportsTotalCount = reportPaths.Count();

            foreach (string reportPath in reportPaths)
            {
                ExportStatus status = null;

                if (!string.IsNullOrEmpty(reportPath))
                {
                    ReportItem reportItem = this.mReportServerReader.GetReport(reportPath);

                    if (reportItem != null)
                    {
                        // Path to where to export report on disk
                        string saveFilePath = exportPath + SSRSUtil.GetServerPathToPhysicalPath(reportPath, "rdl");

                        status = this.mReportExporter.SaveItem(reportItem,
                            saveFilePath,
                            true);

                        this.mLogger.Debug("ExportItemsWorker - ReportItem.Success = {0}; FromPath = {1}; ToPath = {2}",
                            status.Success,
                            status.FromPath,
                            status.ToPath);

                        if (status.Success)
                            ++reportsExportedCounter;
                    }
                    else
                        this.mLogger.Warn("ExportItemsWorker - ReportItem for path '{0}' returned NULL.", reportPath);
                }

                // Always report progress, even if a ListViewItem has an empty path and even if the item isn't retrieved by ReportServerReader.
                // This will keep the progress bar value from suddenly jumping up several values.
                if (worker != null)
                    worker.ReportProgress(((++progressCounter*100)/totalItems), status);
                else
                {
                    this.mLogger.Warn("ExportItemsWorker - worker is NULL.");
                }
            }

            // Stop stopwatch and get how long it took for the export to complete successfully
            watch.Stop();
            double averageItem = watch.Elapsed.TotalSeconds / progressCounter;

            string result = string.Format("{0}/{1} folders, {2}/{3} data sources, {4}/{5} reports exported in {6}h {7}m {8}s (@ {9:0.00} items/s)",
                foldersExportedCounter,
                foldersTotalCount,
                dataSourcesExportedCounter,
                dataSourcesTotalCount,
                reportsExportedCounter,
                reportsTotalCount,
                watch.Elapsed.Hours,
                watch.Elapsed.Minutes,
                watch.Elapsed.Seconds,
                averageItem);

            this.mLogger.Info("ExportItemsWorker - {0}", result);

            e.Result = result;
        }

        private void bw_ExportItems_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            string msg = null;

            if ((e.Cancelled == true))
            {
                msg = string.Format("Cancelled. {0}", e.Result);
            }
            else if ((e.Error != null))
            {
                msg = string.Format("Error. {0}", e.Error);

                this.mLogger.Error(e.Error, "Error during export");

                this.mDebugForm.LogMessage(msg, e.Error);

                MessageBox.Show(msg,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                msg = string.Format("Completed. {0}.", e.Result);                    
            }

            this.mLogger.Info("Export completed: {0}", msg);
            this.lblStatus.Text = msg;
            this.btnSrcRefreshReports.Enabled = true;
            this.btnExport.Enabled = false;
            this.lblStatus.Text = msg;

            this.mSummaryForm.ShowDialog();
        }

        private void bw_ExportItems_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar.Value = e.ProgressPercentage;
            this.progressBar.Maximum = 100;
            this.progressBar.ToolTipText = string.Format("{0} %", e.ProgressPercentage);

            if (e.UserState != null)
            {
                ExportStatus exportStatus = (ExportStatus)e.UserState;

                if (exportStatus.Success)
                {
                    string msg = string.Format("Saved item from '{0}' to '{1}'.",
                        exportStatus.FromPath,
                        exportStatus.ToPath);

                    this.mDebugForm.LogMessage(msg);
                    this.mLogger.Info("ExportItemsProgressChanged - {0}", msg);

                    this.lblStatus.Text = msg;

                    this.mSummaryForm.IncrementSuccessfulItemsCount();
                }
                else
                {
                    string msg = string.Format("Failed to export item from '{0}' to '{1}': {2}",
                        exportStatus.FromPath,
                        exportStatus.ToPath,
                        string.Join(",", exportStatus.Errors));

                    this.mDebugForm.LogMessage(msg, true);
                    this.mLogger.Error(msg);
                    this.lblStatus.Text = string.Format("Failed '{0}': {1}",
                        exportStatus.ToPath,
                        string.Join(",", exportStatus.Errors));

                    this.mSummaryForm.AddFailedItem(exportStatus.FromPath, string.Join(",", exportStatus.Errors));
                }

                this.mSummaryForm.IncrementTotalItemsCount();
            }
            else
                this.mLogger.Warn("ExportItemsProgressChanged - ExportStatus is NULL.");
        }
        #endregion
    }
}
