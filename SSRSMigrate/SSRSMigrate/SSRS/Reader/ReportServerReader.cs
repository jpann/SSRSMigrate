﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSRSMigrate.SSRS.Item;
using SSRSMigrate.SSRS.Repository;

namespace SSRSMigrate.SSRS.Reader
{
    public class ReportServerReader : IReportServerReader
    {
        private IReportServerRepository mReportRepository;

        public ReportServerReader(IReportServerRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            this.mReportRepository = repository;
        }

        #region Folder Methods
        public List<FolderItem> GetFolders(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            List<FolderItem> folders = this.mReportRepository.GetFolders(path);

            return folders;
        }

        public void GetFolders(string path, Action<FolderItem> progressReporter)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            if (progressReporter == null)
                throw new ArgumentNullException("progressReporter");

            var folders = this.mReportRepository.GetFolderList(path);

            foreach (FolderItem folder in folders)
                progressReporter(folder);
        }
        #endregion

        #region Report Methods
        public ReportItem GetReport(string reportPath)
        {
            if (string.IsNullOrEmpty(reportPath))
                throw new ArgumentException("reportPath");

            ReportItem report = this.mReportRepository.GetReport(reportPath);

            return report;
        }

        public List<ReportItem> GetReports(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            return this.mReportRepository.GetReports(path);
        }

        public void GetReports(string path, Action<ReportItem> progressReporter)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            if (progressReporter == null)
                throw new ArgumentNullException("progressReporter");

            var reports = this.mReportRepository.GetReportsList(path);

            foreach (ReportItem report in reports)
                progressReporter(report);
        }
        #endregion

        #region Data Source Methods
        public DataSourceItem GetDataSource(string dataSourcePath)
        {
            if (string.IsNullOrEmpty(dataSourcePath))
                throw new ArgumentException("dataSourcePath");

            DataSourceItem dataSource = this.mReportRepository.GetDataSource(dataSourcePath);

            return dataSource;
        }

        public List<DataSourceItem> GetDataSources(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            return this.mReportRepository.GetDataSources(path);
        }

        public void GetDataSources(string path, Action<DataSourceItem> progressReporter)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            if (progressReporter == null)
                throw new ArgumentNullException("progressReporter");

            var dataSources = this.mReportRepository.GetDataSourcesList(path);

            foreach (DataSourceItem dataSource in dataSources)
                progressReporter(dataSource);
        }
        #endregion
    }
}