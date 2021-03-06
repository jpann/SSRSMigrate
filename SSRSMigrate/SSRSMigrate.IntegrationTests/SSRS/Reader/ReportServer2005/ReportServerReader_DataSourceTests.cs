﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using NUnit.Framework;
using SSRSMigrate.SSRS.Reader;
using SSRSMigrate.SSRS.Item;

namespace SSRSMigrate.IntegrationTests.SSRS.Reader.ReportServer2005
{
    [TestFixture, Category("ConnectsToSSRS")]
    [CoverageExcludeAttribute]
    [Ignore("ReportServer2005 no longer tested.")]
    class ReportServerReader_DataSourceTests
    {
        IReportServerReader reader = null;

        #region GetDataSources - Expected DataSourceItems
        List<DataSourceItem> expectedDataSourceItems = null;
        #endregion

        #region GetDataSources - Actual DataSourceItems
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
                    Description = null,
                    VirtualPath = null,
                    Name = "AWDataSource",
                    Path = "/SSRSMigrate_AW_Tests/Data Sources/AWDataSource",
                    ConnectString = "Data Source=(local);Initial Catalog=AdventureWorks2008",
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
                    Description = null,
                    VirtualPath = null,
                    Name = "Test Data Source",
                    Path = "/SSRSMigrate_AW_Tests/Data Sources/Test Data Source",
                    ConnectString = "Data Source=(local);Initial Catalog=AdventureWorks2008",
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

            reader = TestKernel.Instance.Get<IReportServerReader>("2005-SRC");
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            reader = null;
        }
        
        [SetUp]
        public void SetUp()
        {
            actualDataSourceItems = new List<DataSourceItem>();
        }

        [TearDown]
        public void TearDown()
        {
            actualDataSourceItems = null;
        }

        #region GetDataSource Tests
        [Test]
        public void GetDataSourceItem()
        {
            string dsPath = "/SSRSMigrate_AW_Tests/Data Sources/AWDataSource";

            DataSourceItem actual = reader.GetDataSource(dsPath);

            Assert.AreEqual(expectedDataSourceItems[0].Name, actual.Name);
            Assert.AreEqual(expectedDataSourceItems[0].Path, actual.Path);
            Assert.AreEqual(expectedDataSourceItems[0].ConnectString, actual.ConnectString);
            Assert.AreEqual(expectedDataSourceItems[0].Description, actual.Description);
            Assert.AreEqual(expectedDataSourceItems[0].CredentialsRetrieval, actual.CredentialsRetrieval);
            Assert.AreEqual(expectedDataSourceItems[0].Enabled, actual.Enabled);
            Assert.AreEqual(expectedDataSourceItems[0].EnabledSpecified, actual.EnabledSpecified);
            Assert.AreEqual(expectedDataSourceItems[0].Extension, actual.Extension);
            Assert.AreEqual(expectedDataSourceItems[0].ImpersonateUser, actual.ImpersonateUser);
            Assert.AreEqual(expectedDataSourceItems[0].ImpersonateUserSpecified, actual.ImpersonateUserSpecified);
            Assert.AreEqual(expectedDataSourceItems[0].OriginalConnectStringExpressionBased, actual.OriginalConnectStringExpressionBased);
            Assert.AreEqual(expectedDataSourceItems[0].Password, actual.Password);
            Assert.AreEqual(expectedDataSourceItems[0].Prompt, actual.Prompt);
            Assert.AreEqual(expectedDataSourceItems[0].UseOriginalConnectString, actual.UseOriginalConnectString);
            Assert.AreEqual(expectedDataSourceItems[0].UserName, actual.UserName);
            Assert.AreEqual(expectedDataSourceItems[0].WindowsCredentials, actual.WindowsCredentials);
        }

        [Test]
        public void GetDataSourceItem_NullPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSource(null);
                });

            Assert.That(ex.Message, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        public void GetDataSourceItem_EmptyPath()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(
                delegate
                {
                    reader.GetDataSource("");
                });

            Assert.That(ex.Message, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        public void GetDataSourceItem_PathDoesntExist()
        {
            string dsPath = "/SSRSMigrate_AW_Tests/Test Data Source Doesnt Exist";

            DataSourceItem actual = reader.GetDataSource(dsPath);

            Assert.IsNull(actual);
        }
        #endregion

        #region GetDataSources Tests
        [Test]
        public void GetDataSources()
        {
            string path = "/SSRSMigrate_AW_Tests";

            List<DataSourceItem> actual = reader.GetDataSources(path);

            Assert.AreEqual(expectedDataSourceItems.Count(), actual.Count());
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
        public void GetDataSources_PathDoesntExist()
        {
            string path = "/SSRSMigrate_AW_Tests Doesnt Exist";

            Assert.That(() => reader.GetDataSources(path), Throws.TypeOf<System.Web.Services.Protocols.SoapException>());
        }
        #endregion

        #region GetDataSources Using Action<DataSourceItem> Tests
        [Test]
        public void GetDataSources_UsingDelegate()
        {
            string path = "/SSRSMigrate_AW_Tests";

            reader.GetDataSources(path, GetDataSources_Reporter);

            Assert.AreEqual(expectedDataSourceItems.Count(), actualDataSourceItems.Count());
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
        public void GetDataSources_UsingDelegate_PathDoesntExist()
        {
            string path = "/SSRSMigrate_AW_Tests Doesnt Exist";

            Assert.That(() =>  reader.GetDataSources(path, GetDataSources_Reporter), Throws.TypeOf<System.Web.Services.Protocols.SoapException>());
        }

        public void GetDataSources_Reporter(DataSourceItem dataSource)
        {
            if (dataSource != null)
                actualDataSourceItems.Add(dataSource);
        }
        #endregion
    }
}
