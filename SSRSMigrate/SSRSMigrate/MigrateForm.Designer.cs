﻿namespace SSRSMigrate
{
    partial class MigrateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.grpSource = new System.Windows.Forms.GroupBox();
            this.btnSrcRefreshReports = new System.Windows.Forms.Button();
            this.lstSrcReports = new System.Windows.Forms.ListView();
            this.colSrcName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSrcPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstDestReports = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip.SuspendLayout();
            this.grpSource.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 562);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(917, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(800, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // grpSource
            // 
            this.grpSource.Controls.Add(this.btnSrcRefreshReports);
            this.grpSource.Controls.Add(this.lstSrcReports);
            this.grpSource.Location = new System.Drawing.Point(12, 12);
            this.grpSource.Name = "grpSource";
            this.grpSource.Size = new System.Drawing.Size(441, 513);
            this.grpSource.TabIndex = 1;
            this.grpSource.TabStop = false;
            this.grpSource.Text = "Source Server";
            // 
            // btnSrcRefreshReports
            // 
            this.btnSrcRefreshReports.Location = new System.Drawing.Point(284, 478);
            this.btnSrcRefreshReports.Name = "btnSrcRefreshReports";
            this.btnSrcRefreshReports.Size = new System.Drawing.Size(138, 23);
            this.btnSrcRefreshReports.TabIndex = 15;
            this.btnSrcRefreshReports.Text = "Refresh Reports List";
            this.btnSrcRefreshReports.UseVisualStyleBackColor = true;
            this.btnSrcRefreshReports.Click += new System.EventHandler(this.btnSrcRefreshReports_Click);
            // 
            // lstSrcReports
            // 
            this.lstSrcReports.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSrcName,
            this.colSrcPath});
            this.lstSrcReports.FullRowSelect = true;
            this.lstSrcReports.GridLines = true;
            this.lstSrcReports.Location = new System.Drawing.Point(21, 19);
            this.lstSrcReports.MultiSelect = false;
            this.lstSrcReports.Name = "lstSrcReports";
            this.lstSrcReports.Size = new System.Drawing.Size(401, 453);
            this.lstSrcReports.TabIndex = 14;
            this.lstSrcReports.UseCompatibleStateImageBehavior = false;
            this.lstSrcReports.View = System.Windows.Forms.View.Details;
            // 
            // colSrcName
            // 
            this.colSrcName.Text = "Name";
            this.colSrcName.Width = 161;
            // 
            // colSrcPath
            // 
            this.colSrcPath.Text = "Path";
            this.colSrcPath.Width = 234;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstDestReports);
            this.groupBox1.Location = new System.Drawing.Point(459, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(441, 513);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Destination Server";
            // 
            // lstDestReports
            // 
            this.lstDestReports.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstDestReports.FullRowSelect = true;
            this.lstDestReports.GridLines = true;
            this.lstDestReports.Location = new System.Drawing.Point(18, 19);
            this.lstDestReports.MultiSelect = false;
            this.lstDestReports.Name = "lstDestReports";
            this.lstDestReports.Size = new System.Drawing.Size(401, 453);
            this.lstDestReports.TabIndex = 15;
            this.lstDestReports.UseCompatibleStateImageBehavior = false;
            this.lstDestReports.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 161;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Path";
            this.columnHeader2.Width = 234;
            // 
            // MigrateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 584);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpSource);
            this.Controls.Add(this.statusStrip);
            this.Name = "MigrateForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server-to-Server Migration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.grpSource.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.GroupBox grpSource;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lstSrcReports;
        private System.Windows.Forms.ColumnHeader colSrcName;
        private System.Windows.Forms.ColumnHeader colSrcPath;
        private System.Windows.Forms.Button btnSrcRefreshReports;
        private System.Windows.Forms.ListView lstDestReports;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}
