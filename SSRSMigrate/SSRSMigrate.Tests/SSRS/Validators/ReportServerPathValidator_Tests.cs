using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSRSMigrate.SSRS.Validators;
using SSRSMigrate.TestHelper.Logging;

namespace SSRSMigrate.Tests.SSRS.Validators
{
    [TestFixture]
    class ReportServerPathValidator_Tests
    {
        private ReportServerPathValidator validator = null;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            MockLogger logger = new MockLogger();

            validator = new ReportServerPathValidator();
            validator.Logger = logger;
        }

        #region ValidatePath
        [Test]
        public void ValidPath()
        {
            string path = "/SSRSMigrate_AW_Tests";

            bool actual = validator.Validate(path);

            Assert.IsTrue(actual);
        }

        [Test]
        public void InvalidPath_NULL()
        {
            string path = null;

            bool actual = validator.Validate(path);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidPath_Empty()
        {
            string path = "";

            bool actual = validator.Validate(path);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidPath_LengthExceeded()
        {
            string path = "/SSRSMigrate_AW_Tests";

            path += new String('x', 260);

            bool actual = validator.Validate(path);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidPath_InvalidCharacters()
        {
            string path = "/SSRSMigrate_AW_Tests & Test";

            bool actual = validator.Validate(path);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidPath_InvalidCharacters_Colon()
        {
            string path = "/SSRSMigrate_AW:Tests";

            bool actual = validator.Validate(path);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidPath_ValidCharacters_Period()
        {
            string path = "/SSRSMigrate_AW.Tests";

            bool actual = validator.Validate(path);

            Assert.IsTrue(actual);
        }
        #endregion

        #region ValidateName
        [Test]
        public void ValidName()
        {
            string name = "SSRSMigrate_AW_Tests";

            bool actual = validator.ValidateName(name);

            Assert.IsTrue(actual);
        }

        [Test]
        public void InvalidName_NULL()
        {
            string name = null;

            bool actual = validator.ValidateName(name);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidName_Empty()
        {
            string name = "";

            bool actual = validator.ValidateName(name);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidName_LengthExceeded()
        {
            string name = "SSRSMigrate_AW_Tests";

            name += new String('x', 260);

            bool actual = validator.ValidateName(name);

            Assert.IsFalse(actual);
        }

        [Test]
        public void InvalidName_InvalidCharacters()
        {
            string name = "SSRSMigrate_AW_Tests & Test";

            bool actual = validator.ValidateName(name);

            Assert.IsFalse(actual);
        }
        #endregion
    }
}
