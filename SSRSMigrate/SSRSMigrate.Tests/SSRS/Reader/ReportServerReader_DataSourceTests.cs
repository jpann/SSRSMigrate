﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using SSRSMigrate.SSRS.Reader;
using SSRSMigrate.SSRS.Item;
using SSRSMigrate.SSRS.Repository;
using SSRSMigrate.SSRS.Errors;
using System.Text.RegularExpressions;
using SSRSMigrate.SSRS.Validators;
using SSRSMigrate.TestHelper.Logging;

namespace SSRSMigrate.Tests.SSRS.Reader
{
    [TestFixture]
    [CoverageExcludeAttribute]
    class ReportServerReader_DataSourceTests
    {
        private ReportServerReader reader = null;
        private Mock<IReportServerPathValidator> pathValidatorMock = null;
        private Mock<IReportServerRepository> reportServerRepositoryMock = null;

        #region GetDataSources - Expected DataSourceItems
        List<DataSourceItem> expectedDataSourceItems = null;
        #endregion

        #region GetDatasources - Actual DataSourceItems
        List<DataSourceItem> actualDataSourceItems = null;
        #endregion

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // Setup expected DataSourceItems
            expectedDataSourceItems = new List<DataSourceItem>()
            {
                new DataSourceItem()
                {
                    CreatedBy = "DOMAIN\\user",
                    CreationDate = DateTime.Parse("7/23/2014 8:28:43 AM"),
                    Description = null,
                    ID = Guid.NewGuid().ToString(),
                    ModifiedBy = "DOMAIN\\user",
                    ModifiedDate = DateTime.Parse("7/23/2014 8:28:43 AM"),
                    Size = 414,
                    VirtualPath = null,
                    Name = "AWDataSource",
                    Path = "/SSRSMigrate_AW_Tests/Data Sources/AWDataSource",
                    ConnectString = "Data Source=(local)\\SQL2008;Initial Catalog=AdventureWorks2008",
                    CredentialsRetrieval = "Integrated",
                    Enabled = true,
                    EnabledSpecified = true,
                    Extension = "SQL",
                    ImpersonateUser = false,
                    ImpersonateUserSpecified = true,
                    OriginalConnectStringExpressionBased = false,
                    Password = null,
                    Prompt = "Enter a user name and password to access the data source:",
                    UseOriginalConnectString = false,
                    UserName = null,
                    WindowsCredentials = false
                },
                new DataSourceItem()
                {
                    CreatedBy = "DOMAIN\\user",
                    CreationDate = DateTime.Parse("7/23/2014 8:29:25 AM"),
                    Description = null,
                    ID = Guid.NewGuid().ToString(),
                    ModifiedBy = "DOMAIN\\user",
                    ModifiedDate = DateTime.Parse("7/23/2014 8:29:25 AM"),
                    Size = 414,
                    VirtualPath = null,
                    Name = "Test Data Source",
                    Path = "/SSRSMigrate_AW_Tests/Data Sources/Test Data Source",
                    ConnectString = "Data Source=(local)\\SQL2008;Initial Catalog=AdventureWorks2008",
                    CredentialsRetrieval = "Integrated",
                    Enabled = true,
                    EnabledSpecified = true,
                    Extension = "SQL",
                    ImpersonateUser = false,
                    ImpersonateUserSpecified = true,
                    OriginalConnectStringExpressionBased = false,
                    Password = null,
                    Prompt = "Enter a user name and password to access the data source:",
                    UseOriginalConnectString = false,
                    UserName = null,
                    WindowsCredentials = false
                },
            };
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            reader = null;
        }

        [SetUp]
        public void SetUp()
        {
            // Setup IReportServerRepository mock
            reportServerRepositoryMock = new Mock<IReportServerRepository>();

            // Setup IReportServerPathValidator mock
            pathValidatorMock = new Mock<IReportServerPathValidator>();

            MockLogger logger = new MockLogger();

            reader = new ReportServerReader(reportServerRepositoryMock.Object, logger, pathValidatorMock.Object);

            actualDataSourceItems = new List<DataSourceItem>();
        }

        [TearDown]
        public void TearDown()
        {
            actualDataSourceItems = null;
        }

        #region GetDataSource Tests
        [Test]
        public void GetDataSource()
        {
            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests/Data Sources/AWDataSource"))
                .Returns(() => true);

            reportServerRepositoryMock.Setup(r => r.GetDataSource("/SSRSMigrate_AW_Tests/Data Sources/AWDataSource"))
                .Returns(() => expectedDataSourceItems[0]);

            DataSourceItem actualDataSource = reader.GetDataSource("/SSRSMigrate_AW_Tests/Data Sources/AWDataSource");

            Assert.AreEqual(expectedDataSourceItems[0], actualDataSource);
        }

        [Test]
        public void GetDataSource_PathDoesntExist()
        {
            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests/Data Sources/Test Data Source Doesnt Exist"))
                .Returns(() => true);

            reportServerRepositoryMock.Setup(r => r.GetDataSource("/SSRSMigrate_AW_Tests/Data Sources/Test Data Source Doesnt Exist"))
                .Returns(() => null);

            DataSourceItem actualDataSource = reader.GetDataSource("/SSRSMigrate_AW_Tests/Data Sources/Test Data Source Doesnt Exist");

            Assert.Null(actualDataSource);
        }

        [Test]
        public void GetDataSource_NullPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSource(null);
                });

            Assert.That(ex.Message, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        public void GetDataSource_EmptyPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSource("");
                });

            Assert.That(ex.Message, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        public void GetDataSource_InvalidPath()
        {
            pathValidatorMock.Setup(r => r.Validate(It.Is<string>(s => Regex.IsMatch(s, "[:?;@&=+$,\\*><|.\"]+") == true)))
                .Returns(() => false);

            string invalidPath = "/SSRSMigrate_AW_Tests/Data Sources/Test.Data Source";

            InvalidPathException ex = Assert.Throws<InvalidPathException>(
                delegate
                {
                    reader.GetDataSource(invalidPath);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Invalid path '{0}'.", invalidPath)));
        }
        #endregion

        #region GetDataSources Tests
        [Test]
        public void GetDataSources()
        {
            reportServerRepositoryMock.Setup(r => r.GetDataSources("/SSRSMigrate_AW_Tests"))
                .Returns(() => expectedDataSourceItems);

            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests"))
                .Returns(() => true);

            List<DataSourceItem> dataSourceItems = reader.GetDataSources("/SSRSMigrate_AW_Tests");

            Assert.AreEqual(dataSourceItems.Count(), expectedDataSourceItems.Count());
            Assert.AreEqual(expectedDataSourceItems[0], dataSourceItems[0]);
            Assert.AreEqual(expectedDataSourceItems[1], dataSourceItems[1]);
        }

        [Test]
        public void GetDataSources_PathDoesntExist()
        {
            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests Doesnt Exist"))
                .Returns(() => true);

            reportServerRepositoryMock.Setup(r => r.GetDataSources("/SSRSMigrate_AW_Tests Doesnt Exist"))
                .Returns(() => new List<DataSourceItem>());

            List<DataSourceItem> dataSourceItems = reader.GetDataSources("/SSRSMigrate_AW_Tests Doesnt Exist");

            Assert.AreEqual(0, dataSourceItems.Count());
        }

        [Test]
        public void GetDataSources_NullPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSources(null);
                });

            Assert.That(ex.Message, Is.EqualTo("path"));
        }

        [Test]
        public void GetDataSources_EmptyPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSources("");
                });

            Assert.That(ex.Message, Is.EqualTo("path"));
        }

        [Test]
        public void GetDataSources_InvalidPath()
        {
            pathValidatorMock.Setup(r => r.Validate(It.Is<string>(s => Regex.IsMatch(s ?? "", "[:?;@&=+$,\\*><|.\"]+") == true)))
                .Returns(() => false);

            string invalidPath = "/SSRSMigrate_AW.Tests";

            InvalidPathException ex = Assert.Throws<InvalidPathException>(
                delegate
                {
                    reader.GetDataSources(invalidPath);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Invalid path '{0}'.", invalidPath)));
        }
        #endregion

        #region GetDataSources Using Action<DataSourceItem> Tests
        [Test]
        public void GetDataSources_UsingDelegate()
        {
            reportServerRepositoryMock.Setup(r => r.GetDataSourcesList("/SSRSMigrate_AW_Tests"))
                .Returns(() => expectedDataSourceItems);

            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests"))
                .Returns(() => true);

            reader.GetDataSources("/SSRSMigrate_AW_Tests", GetDataSources_Reporter);

            Assert.AreEqual(expectedDataSourceItems.Count(), actualDataSourceItems.Count());
            Assert.AreEqual(expectedDataSourceItems[0], actualDataSourceItems[0]);
            Assert.AreEqual(expectedDataSourceItems[1], actualDataSourceItems[1]);
        }

        [Test]
        public void GetDataSources_UsingDelegate_PathDoesntExist()
        {
            pathValidatorMock.Setup(r => r.Validate("/SSRSMigrate_AW_Tests Doesnt Exist"))
                .Returns(() => true);

            reportServerRepositoryMock.Setup(r => r.GetDataSourcesList("/SSRSMigrate_AW_Tests Doesnt Exist"))
                .Returns(() => new List<DataSourceItem>());

            reader.GetDataSources("/SSRSMigrate_AW_Tests Doesnt Exist", GetDataSources_Reporter);

            Assert.AreEqual(0, actualDataSourceItems.Count());
        }

        [Test]
        public void GetDataSources_UsingDelegate_NullPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSources(null, GetDataSources_Reporter);
                });

            Assert.That(ex.Message, Is.EqualTo("path"));
        }

        [Test]
        public void GetDataSources_UsingDelegate_EmptyPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSources("", GetDataSources_Reporter);
                });

            Assert.That(ex.Message, Is.EqualTo("path"));
        }

        [Test]
        public void GetDataSources_UsingDelegate_NullDelegate()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
                delegate
                {
                    reader.GetDataSources("/SSRSMigrate_AW_Tests", null);
                });

            Assert.That(ex.Message, Is.EqualTo("Value cannot be null.\r\nParameter name: progressReporter"));
        }

        [Test]
        public void GetDataSources_UsingDelegate_InvalidPath()
        {
            string invalidPath = "/SSRSMigrate_AW.Tests";

            pathValidatorMock.Setup(r => r.Validate(("/SSRSMigrate_AW.Tests")))
                .Returns(() => false);

            InvalidPathException ex = Assert.Throws<InvalidPathException>(
                delegate
                {
                    reader.GetDataSources(invalidPath, GetDataSources_Reporter);
                });

            Assert.That(ex.Message, Is.EqualTo(string.Format("Invalid path '{0}'.", invalidPath)));
        }

        private void GetDataSources_Reporter(DataSourceItem dataSourceItem)
        {
            actualDataSourceItems.Add(dataSourceItem);
        }
        #endregion
    }
}
