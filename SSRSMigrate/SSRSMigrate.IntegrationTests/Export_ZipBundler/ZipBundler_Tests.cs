﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using NUnit.Framework;
using System.Reflection;
using System.IO;
using SSRSMigrate.Bundler;
using SSRSMigrate.Enum;
using SSRSMigrate.Wrappers;
using SSRSMigrate.Exporter;
using Ninject.Extensions.Logging.Log4net;

namespace SSRSMigrate.IntegrationTests.Export_ZipBundler
{
    #region Test Structures
    // Holds data for test methods and mocks
    public struct TestData
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    };
    #endregion

    [TestFixture]
    [CoverageExcludeAttribute]
    class ZipBundler_Tests
    {
        private StandardKernel kernel = null;
        private IBundler zipBundler = null;

        private string zipArchiveFilename = Path.Combine(GetOutPutPath(), "SSRSMigrate_AW_Tests.zip");

        #region Test Data
        TestData awDataSource = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Data Sources\\AWDataSource.json"),
            Path = "/SSRSMigrate_AW_Tests/Data Sources/AWDataSource"
        };

        TestData testDataSource = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Data Sources\\Test Data Source.json"),
            Path = "/SSRSMigrate_AW_Tests/Data Sources/Test Data Source"
        };

        TestData rootFolder = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests"),
            Path = "/SSRSMigrate_AW_Tests"
        };

        TestData dataSourcesFolder = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Data Sources"),
            Path = "/SSRSMigrate_AW_Tests/Data Sources"
        };

        TestData reportsFolder = new TestData()
        {
            FileName =Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Reports"),
            Path = "/SSRSMigrate_AW_Tests/Reports"
        };

        TestData subFolder = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Sub Folder"),
            Path = "/SSRSMigrate_AW_Tests/Sub Folder"
        };

        TestData companySalesReport = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Reports\\Company Sales.rdl"),
            Path = "/SSRSMigrate_AW_Tests/Reports/Company Sales"
        };

        TestData salesOrderDetailReport = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Reports\\Sales Order Detail.rdl"),
            Path = "/SSRSMigrate_AW_Tests/Reports/Sales Order Detail"
        };

        TestData storeContactsReport = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Reports\\Store Contacts.rdl"),
            Path = "/SSRSMigrate_AW_Tests/Reports/Store Contacts"
        };

        TestData doesNotExistReport = new TestData()
        {
            FileName =Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Reports\\File Doesnt Exist.rdl"),
            Path = "/SSRSMigrate_AW_Tests/Reports/File Doesnt Exist"
        };

        TestData folderDesNotExistReport = new TestData()
        {
            FileName = Path.Combine(GetInputPath(), "SSRSMigrate_AW_Tests\\Folder Doesnt Exist"),
            Path = "/SSRSMigrate_AW_Tests/Folder Doesnt Exist"
        };
        #endregion

        #region Expected Values
        private string expectedeSourceRootPath = "/SSRSMigrate_AW_Tests";
        private SSRSVersion expectedSourceVersion = SSRSVersion.SqlServer2008R2;

        string expectedSummary = @"{
  ""SourceRootPath"": ""/SSRSMigrate_AW_Tests"",
  ""SourceVersion"": ""SqlServer2008R2"",
  ""Entries"": {
    ""DataSources"": [
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Data Sources"",
        ""FileName"": ""AWDataSource.json"",
        ""CheckSum"": ""7b4e44d94590f501ba24cd3904a925c3""
      }
    ],
    ""Reports"": [
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Reports"",
        ""FileName"": ""Company Sales.rdl"",
        ""CheckSum"": ""1adde7720ca2f0af49550fc676f70804""
      },
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Reports"",
        ""FileName"": ""Sales Order Detail.rdl"",
        ""CheckSum"": ""640a2f60207f03779fdedfed71d8101d""
      },
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Reports"",
        ""FileName"": ""Store Contacts.rdl"",
        ""CheckSum"": ""a225b92ed8475e6bc5b59f5b2cc396fa""
      }
    ],
    ""Folders"": [
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests"",
        ""FileName"": """",
        ""CheckSum"": """"
      },
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Data Sources"",
        ""FileName"": """",
        ""CheckSum"": """"
      },
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Reports"",
        ""FileName"": """",
        ""CheckSum"": """"
      },
      {
        ""Path"": ""Export\\SSRSMigrate_AW_Tests\\Sub Folder"",
        ""FileName"": """",
        ""CheckSum"": """"
      }
    ]
  }
}";

        string expectedSummaryNoData = @"{
  ""SourceRootPath"": ""/SSRSMigrate_AW_Tests"",
  ""SourceVersion"": ""SqlServer2008R2"",
  ""Entries"": {
    ""DataSources"": [],
    ""Reports"": [],
    ""Folders"": []
  }
}";
        #endregion

        #region Environment Methods
        // Static so they can be used in field initializers
        private static string GetOutPutPath()
        {
            // Use the test assembly's directory instead of where nunit runs the test
            string outputPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            outputPath = outputPath.Replace("file:\\", "");

            return Path.Combine(outputPath, "ZipBundler_Output");
        }

        private static string GetInputPath()
        {
            // Use the test assembly's directory instead of where nunit runs the test
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            dir = dir.Replace("file:\\", "");

            return Path.Combine(dir, "Test AW Data\\ZipBundler");
        }

        private void SetUpEnvironment()
        {
            if (Directory.Exists(GetOutPutPath()))
                this.TearDownEnvironment();

            // Create output directory
            Directory.CreateDirectory(GetOutPutPath());
        }

        private void TearDownEnvironment()
        {
            // Delete output directory if it exists
            if (Directory.Exists(GetOutPutPath()))
                Directory.Delete(GetOutPutPath(), true);

            if (File.Exists(zipArchiveFilename))
                File.Delete(zipArchiveFilename);
        }
        #endregion

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            var settings = new NinjectSettings()
            {
                LoadExtensions = false
            };

            kernel = new StandardKernel(
                settings,
                new Log4NetModule(),
                new DependencyModule());
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {

        }

        [SetUp]
        public void SetUp()
        {
            // Each test will add files to ZipFileWrapper using ZipBundler,
            //  so they need to get recreated for each test so the zip is in a clean state
            //  for each test.
            zipBundler = kernel.Get<IBundler>();

            this.SetUpEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            zipBundler = null;

            this.TearDownEnvironment();
        }

        #region GetZipPath Tests
        [Test]
        public void GetZipPath_File()
        {
            string itemFileName = awDataSource.FileName;
            string itemPath = awDataSource.Path;

            string expectedZipPath = "Export\\SSRSMigrate_AW_Tests\\Data Sources";

            string actual = zipBundler.GetZipPath(itemFileName, itemPath);
            Assert.AreEqual(expectedZipPath, actual);
        }

        [Test]
        public void GetZipPath_File_NullFileName()
        {
            string itemPath = awDataSource.Path;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.GetZipPath(null, itemPath);
                });

            Assert.That(ex.Message, Is.EqualTo("itemFileName"));
        }

        [Test]
        public void GetZipPath_File_EmptyFileName()
        {
            string itemPath = awDataSource.Path;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.GetZipPath("", itemPath);
                });

            Assert.That(ex.Message, Is.EqualTo("itemFileName"));
        }

        [Test]
        public void GetZipPath_File_NullPath()
        {
            string itemFileName = awDataSource.FileName;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.GetZipPath(itemFileName, null);
                });

            Assert.That(ex.Message, Is.EqualTo("itemPath"));
        }

        [Test]
        public void GetZipPath_File_EmptyPath()
        {
            string itemFileName = awDataSource.FileName;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.GetZipPath(itemFileName, "");
                });

            Assert.That(ex.Message, Is.EqualTo("itemPath"));
        }

        [Test]
        public void GetZipPath_File_InvalidPath()
        {
            string itemFileName = awDataSource.FileName;
            string itemPath = "/SSRSMigrate/Data Sources/AWDataSource"; // This path is not contained within awDataSource.FileName, so it is invalid

            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    zipBundler.GetZipPath(itemFileName, itemPath);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Item path '{0}' is invalid.", itemPath)));

        }
        #endregion

        #region CreateEntrySummary Tests
        /// <summary>
        /// Test CreateEntrySummary for a file by passing in valid values.
        /// </summary>
        [Test]
        public void CreateEntrySummary_File()
        {
            string itemFileName = awDataSource.FileName;
            string expectedZipPath = "Export\\SSRSMigrate_AW_Tests\\Data Sources";
            string expectedFileName = "AWDataSource.json";
            string expectedCheckSum = "7b4e44d94590f501ba24cd3904a925c3";

            BundleSummaryEntry actual = zipBundler.CreateEntrySummary(itemFileName, expectedZipPath);

            Assert.NotNull(actual);
            Assert.AreEqual(expectedCheckSum, actual.CheckSum, itemFileName);
            Assert.AreEqual(expectedZipPath, actual.Path);
            Assert.AreEqual(expectedFileName, actual.FileName);
        }

        /// <summary>
        /// Test CreateEntrySummary for a file by passing in null value for itemFileName
        /// </summary>
        [Test]
        public void CreateEntrySummary_File_NullFileName()
        {
            string zipPath = "Export\\SSRSMigrate_AW_Tests\\Data Sources";

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.CreateEntrySummary(null, zipPath);
                });

            Assert.That(ex.Message, Is.EqualTo("itemFileName"));
        }

        /// <summary>
        /// Test CreateEntrySummary for a file by passing in an empty string for itemFileName
        /// </summary>
        [Test]
        public void CreateEntrySummary_File_EmptyFileName()
        {
            string zipPath = "Export\\SSRSMigrate_AW_Tests\\Data Sources";

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.CreateEntrySummary("", zipPath);
                });

            Assert.That(ex.Message, Is.EqualTo("itemFileName"));
        }

        /// <summary>
        /// Test CreateEntrySummary for a file by passing in null value for zipPath
        /// </summary>
        [Test]
        public void CreateEntrySummary_File_NullPath()
        {
            string itemFileName = awDataSource.FileName;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.CreateEntrySummary(itemFileName, null);
                });

            Assert.That(ex.Message, Is.EqualTo("zipPath"));
        }

        /// <summary>
        /// Test CreateEntrySummary for a file by passing in an empty string for zipPath
        /// </summary>
        [Test]
        public void CreateEntrySummary_File_EmptyPath()
        {
            string itemFileName = awDataSource.FileName;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.CreateEntrySummary(itemFileName, "");
                });

            Assert.That(ex.Message, Is.EqualTo("zipPath"));
        }

        /// <summary>
        /// Test CreateEntrySummary for a file that does not exist
        /// </summary>
        [Test]
        public void CreateEntrySummary_File_DoesntExist()
        {
            string itemFileName = doesNotExistReport.FileName;
            string zipPath = "Export\\SSRSMigrate_AW_Tests\\Reports";
            string expectedFileName = "File Doesnt Exist.rdl";

            FileNotFoundException ex = Assert.Throws<FileNotFoundException>(
               delegate
               {
                   BundleSummaryEntry actual = zipBundler.CreateEntrySummary(itemFileName, zipPath);
               });

            Assert.That(ex.Message, Is.EqualTo(itemFileName));
        }
        #endregion

        #region AddItem DataSource Tests
        [Test]
        public void AddItem_DataSource()
        {
            zipBundler.AddItem("DataSources",
                awDataSource.FileName,
                awDataSource.Path,
                false);

            // Check that the ZipBundler has the entry we added to DataSources
            Assert.NotNull(zipBundler.Summary.Entries["DataSources"][0]);

            // Check that the proper ZipPath exists in the DataSource entry we added
            Assert.AreEqual(
                "Export\\SSRSMigrate_AW_Tests\\Data Sources",
                zipBundler.Summary.Entries["DataSources"][0].Path);

            // Check that the checksum is correct for the DataSource entry we added
            Assert.AreEqual(
                "7b4e44d94590f501ba24cd3904a925c3",
                zipBundler.Summary.Entries["DataSources"][0].CheckSum);

            // Check that the filename is correct for the DataSource entry we added
            Assert.AreEqual(
                "AWDataSource.json",
                zipBundler.Summary.Entries["DataSources"][0].FileName);

            // Check that the DataSource file exists in ZipFileWrapper
            //Assert.True(zipFileWrapper.FileExists("Export\\SSRSMigrate_AW_Tests\\Data Sources\\AWDataSource.json"));
        }

        [Test]
        public void AddItem_DataSource_InvalidItemPath()
        {
            string itemFileName = awDataSource.FileName;
            string itemPath = "/SSRSMigrate/Data Sources/AWDataSource"; // This path is not contained within awDataSource.FileName, so it is invalid

            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    zipBundler.AddItem("DataSources",
                        itemFileName,
                        itemPath,
                        false);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Item path '{0}' is invalid.", itemPath)));
        }

        [Test]
        public void AddItem_DataSource_FileAsDirectory()
        {
            DirectoryNotFoundException ex = Assert.Throws<DirectoryNotFoundException>(
                delegate
                {
                    zipBundler.AddItem("DataSources",
                        awDataSource.FileName,
                        awDataSource.Path,
                        true); // Add to zip as directory
                });

            Assert.That(ex.Message, Is.EqualTo(awDataSource.FileName));
        }
        #endregion

        #region AddItem Folder Tests
        [Test]
        public void AddItem_Folder()
        {
            zipBundler.AddItem("Folders",
                reportsFolder.FileName,
                reportsFolder.Path,
                true);

            // Check that the ZipBunlder has the entry we added to Folders
            Assert.NotNull(zipBundler.Summary.Entries["Folders"][0]);

            // Check that the proper ZipPath exists in the Folder entry we added
            Assert.AreEqual(
                "Export\\SSRSMigrate_AW_Tests\\Reports",
                zipBundler.Summary.Entries["Folders"][0].Path);

            // Check that the checksum is correct for the Folder entry we added
            Assert.AreEqual(
                "",
                zipBundler.Summary.Entries["Folders"][0].CheckSum);

            // Check that the filename is correct for the Folder entry we added
            Assert.AreEqual(
                "",
                zipBundler.Summary.Entries["Folders"][0].FileName);

            // Check that the Folder exists in ZipFileWrapper
            //Assert.True(zipFileWrapper.FileExists("Export\\SSRSMigrate_AW_Tests\\Reports"));
        }

        [Test]
        public void AddItem_Folder_InvalidItemPath()
        {
            string itemFileName = reportsFolder.FileName;
            string itemPath = "/SSRSMigrate/Reports"; // This path is not contained within reportsFolder.FileName, so it is invalid

            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    zipBundler.AddItem("Folders",
                        itemFileName,
                        itemPath,
                        true);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Item path '{0}' is invalid.", itemPath)));
        }
        #endregion

        #region AddItem Report Tests
        [Test]
        public void AddItem_Report()
        {
            zipBundler.AddItem("Reports",
                companySalesReport.FileName,
                companySalesReport.Path,
                false);

            // Check that ZipBundler has the entry we added to Reports
            Assert.NotNull(zipBundler.Summary.Entries["Reports"][0]);

            // Check that the proper ZipPath exists in the Reports entry we added
            Assert.AreEqual(
                "Export\\SSRSMigrate_AW_Tests\\Reports",
                zipBundler.Summary.Entries["Reports"][0].Path);

            // Check that the checksum is correct fot the Report entry we added
            Assert.AreEqual(
                "1adde7720ca2f0af49550fc676f70804",
                zipBundler.Summary.Entries["Reports"][0].CheckSum);

            // Check that the filename is correct for the Report entry we added
            Assert.AreEqual(
                "Company Sales.rdl",
                zipBundler.Summary.Entries["Reports"][0].FileName);

            // Check that the Report exists in ZipFileWrapper
            //Assert.True(zipFileWrapper.FileExists("Export\\SSRSMigrate_AW_Tests\\Reports\\Company Sales.rdl"));
        }

        [Test]
        public void AddItem_Report_InvalidItemPath()
        {
            string itemFileName = companySalesReport.FileName;
            string itemPath = "/SSRSMigrate/Reports/Company Sales"; // This path is not contained within companySalesReport.FileName, so it is invalid

            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    zipBundler.AddItem("Reports",
                        itemFileName,
                        itemPath,
                        false);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Item path '{0}' is invalid.", itemPath)));
        
        }
        #endregion

        #region AddItem Directory as File
        /// <summary>
        /// Tests AddItem by passing it a directory but with isFolder boolean value of False
        /// </summary>
        [Test]
        public void AddItem_DirectoryAsFile()
        {
            FileNotFoundException ex = Assert.Throws<FileNotFoundException>(
                delegate
                {
                    zipBundler.AddItem("Reports",
                        subFolder.FileName,
                        subFolder.Path,
                        false); // Add to zip as file
                });

            Assert.That(ex.Message, Is.EqualTo(subFolder.FileName));
        }
        #endregion

        #region CreateSummary Tests
        [Test]
        public void CreateSummary()
        {
            // Add test data to ZipBundler
            // Add Data Source item to ZipBundler
            zipBundler.AddItem("DataSources",
                awDataSource.FileName,
                awDataSource.Path,
                false);

            // Add Company Sales report
            zipBundler.AddItem("Reports",
                companySalesReport.FileName,
                companySalesReport.Path,
                false);

            // Add Sales Order Detail report
            zipBundler.AddItem("Reports",
                salesOrderDetailReport.FileName,
                salesOrderDetailReport.Path,
                false);

            // Add Store Cotnacts [sub] report
            zipBundler.AddItem("Reports",
               storeContactsReport.FileName,
               storeContactsReport.Path,
               false);

            // Add folder items to ZipBundle last since they will automatically get added by each item,
            // so this will prevent duplicate key exceptions
            // Add root  folder item
            zipBundler.AddItem("Folders",
                rootFolder.FileName,
                rootFolder.Path,
                true);

            // Add data sources folder folder item
            zipBundler.AddItem("Folders",
               dataSourcesFolder.FileName,
               dataSourcesFolder.Path,
               true);

            // Add reports folder item
            zipBundler.AddItem("Folders",
               reportsFolder.FileName,
               reportsFolder.Path,
               true);

            // Add sub folder folder item
            zipBundler.AddItem("Folders",
                subFolder.FileName,
                subFolder.Path,
                true);

            string actual = zipBundler.CreateSummary(expectedeSourceRootPath, expectedSourceVersion);

            Assert.AreEqual(expectedSummary, actual);
        }

        [Test]
        public void CreateSummary_NoData()
        {
            string actual = zipBundler.CreateSummary(expectedeSourceRootPath, expectedSourceVersion);

            Assert.AreEqual(expectedSummaryNoData, actual);
        }
        #endregion

        #region Save Tests
        [Test]
        public void Save()
        {
            string filename = zipArchiveFilename;

            zipBundler.Save(filename);

            Assert.True(File.Exists(filename));
        }

        [Test]
        public void Save_NullFileName()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.Save(null);
                });

            Assert.That(ex.Message, Is.EqualTo("fileName"));
        }

        [Test]
        public void Save_EmptyFileName()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    zipBundler.Save("");
                });

            Assert.That(ex.Message, Is.EqualTo("fileName"));
        }
        #endregion

        #region Save Bundle Tests
        [Test]
        public void Save_CompleteBundle()
        {
            // Add test data to ZipBundler
            // Add Data Source item to ZipBundler
            zipBundler.AddItem("DataSources",
                awDataSource.FileName,
                awDataSource.Path,
                false);

            // Add Folder item to ZipBundler
            zipBundler.AddItem("Folders",
                rootFolder.FileName,
                rootFolder.Path,
                true);

            // Add Report items to ZipBundler
            // Add Company Sales report
            zipBundler.AddItem("Reports",
                companySalesReport.FileName,
                companySalesReport.Path,
                false);

            // Add Sales Order Detail report
            zipBundler.AddItem("Reports",
                salesOrderDetailReport.FileName,
                salesOrderDetailReport.Path,
                false);

            // Add Store Cotnacts [sub] report
            zipBundler.AddItem("Reports",
               storeContactsReport.FileName,
               storeContactsReport.Path,
               false);

            // Create and add summary to ZipBundler
            zipBundler.CreateSummary(expectedeSourceRootPath, expectedSourceVersion);
            
            // Save archive to disk
            string filename = zipArchiveFilename;

            zipBundler.Save(filename);

            Assert.True(File.Exists(filename));
        }
        #endregion
    }
}
