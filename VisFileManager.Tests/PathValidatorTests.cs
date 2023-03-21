using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisFileManager.Validators;

namespace VisFileManager.Tests
{
    [TestClass]
    public class PathValidatorTests
    {
        const string LocalDownloadsFolder = "C:\\Users\\Martin Chrzan\\Downloads";

        [TestMethod]
        [DataRow("C:", "C:\\", PathValidator.PathType.Directory)]
        [DataRow("%programdata%", "C:\\ProgramData", PathValidator.PathType.Directory)]
        [DataRow("test", "", PathValidator.PathType.Invalid)]
        [DataRow("C:\\Program Files\\", "C:\\Program Files", PathValidator.PathType.Directory)]
        [DataRow("", "", PathValidator.PathType.MyComputer)]
        [DataRow(":\\ProgramData", "", PathValidator.PathType.Invalid)]
        [DataRow("programdata", "C:\\ProgramData", PathValidator.PathType.Directory)]
        [DataRow("downloads", LocalDownloadsFolder, PathValidator.PathType.Directory)]
        public void PathValidatorTest(string input, string expectedFormattedOutput, PathValidator.PathType expectedPathType)
        {
            var formatted = string.Empty;
            var resultPathType = PathValidator.ValidatePath(input, out formatted);
            Assert.AreEqual(expectedFormattedOutput, formatted);
            Assert.AreEqual(expectedPathType, resultPathType);
        }
    }
}
