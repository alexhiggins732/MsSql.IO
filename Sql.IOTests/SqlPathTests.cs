using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sql.IO;
using Sql.IOTests;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO.Tests
{
    [TestClass()]
    public class SqlPathTests
    {
        const string backslash = @"\";
        public string uncRoot;
        public string dbRoot;
        public string root;
        public string fileTableDirectory;
        public string fileTablePath;
        public string absoluteFileTablePath;
        public string relativeFilePath;
        public string absoluteRelativePath;
        public string fullPath;
        public SqlPathTests(IUncInfo uncInfo)
        {
            uncRoot = $@"\\{uncInfo.UncServerName}\{uncInfo.InstanceDirectory}";
            dbRoot = $@"{uncInfo.DatabaseDirectory}";
            root = Path.Combine(uncRoot, dbRoot);
            fileTableDirectory = $@"{uncInfo.FileTableDirectory}";
            fileTablePath = Path.Combine(root, fileTableDirectory);
            absoluteFileTablePath = @"\" + fileTableDirectory;
            relativeFilePath = @"ExplorerCreated\FileTableGist.sql";
            absoluteRelativePath = Path.Combine(fileTableDirectory, relativeFilePath);
            fullPath = Path.Combine(root, absoluteRelativePath);
        }

        [TestMethod()]
        public void GetUncRootTest()
        {
            var uncRoot_1 = SqlPath.GetUncRoot(fullPath);
            var uncRoot_2 = SqlPath.GetUncRoot(uncRoot);
            var uncRoot_3 = SqlPath.GetUncRoot(uncRoot + backslash);
        }

        [TestMethod()]
        public void GetFileStreamDirectoryTest()
        {
            var fileStreamDirectory_1 = SqlPath.GetFileStreamDirectory(root);
            var fileStreamDirectory_2 = SqlPath.GetFileStreamDirectory(root + backslash);
            var fileStreamDirectory_3 = SqlPath.GetFileStreamDirectory(fullPath);
        }

        [TestMethod()]
        public void GetFileTableDirectoryTest()
        {
            var fileTableDirectory_1 = SqlPath.GetFileTableDirectory(fileTablePath);
            var fileTableDirectory_2 = SqlPath.GetFileTableDirectory(fileTablePath + backslash);
            var fileTableDirectory_3 = SqlPath.GetFileTableDirectory(fullPath);
        }

        [TestMethod()]
        public void GetFileTableRelativePathTest()
        {
            var fileTableDirectoryRelative_1 = SqlPath.GetFileTableRelativePath(fileTablePath);
            var fileTableDirectoryRelative_2 = SqlPath.GetFileTableRelativePath(fileTablePath + backslash);
            var fileTableDirectoryRelative_3 = SqlPath.GetFileTableRelativePath(fullPath);
        }

        [TestMethod()]
        public void IsPathRootedTest()
        {
            var isRooted_root = Path.IsPathRooted(root);
            var isRooted_relative = Path.IsPathRooted(absoluteRelativePath);
            var isRooted_full = Path.IsPathRooted(fullPath);
        }

        [TestMethod()]
        public void GetRootPathTest()
        {
            var rootPath_root = Path.GetPathRoot(root);
            var rootPath_relative = Path.GetPathRoot(absoluteRelativePath);
            var rootPath = Path.GetPathRoot(fullPath);
        }

        [TestMethod()]
        public void GetFileSystemInfoTest()
        {
            //var fileSystemInfo = SqlPath.GetFileSystemInfo(fullPath);
            //Assert.IsNotNull(fileSystemInfo);
            //Assert.IsFalse(fileSystemInfo.Is_Directory);
            //Assert.IsTrue(fileSystemInfo.FullName == fullPath);
            var fileInfo = new SqlFileInfo(fullPath);

            Assert.IsNotNull(fileInfo);
            Assert.IsFalse(fileInfo.Is_Directory);
            Assert.IsFalse(fileInfo.Exists);
            Assert.IsTrue(fileInfo.FullName == fullPath);
            Assert.IsFalse(fileInfo.Directory.Exists);


            fileInfo.Directory.Create();
            Assert.IsTrue(fileInfo.Directory.Exists);

            var writeText = new System.Net.WebClient().DownloadString("https://gist.githubusercontent.com/alexhiggins732/acd66090322358aa2e510c02f642a0c9/raw");
            SqlFile.WriteAllText(fileInfo, writeText);
            Assert.IsTrue(fileInfo.Exists);


            using(var sw= new System.IO.StreamReader(fileInfo.File_Stream()))
            {
                var readText = sw.ReadToEnd();
                Assert.IsTrue(readText == writeText);
            }

            //var fi = new FileInfo(fullPath);
            //var di = fi.Directory;
            //di.Create();
            //fi.Create();
            SqlDirectoryInfo directoryInfo = fileInfo.Directory;

            Assert.IsNotNull(directoryInfo);
            Assert.IsTrue(directoryInfo.Is_Directory);
            Assert.IsTrue(directoryInfo.Exists);

            string directoryFullName = directoryInfo.FullName;

            var directoryInfo2 = new SqlDirectoryInfo(directoryFullName);
            Assert.IsNotNull(directoryInfo2);
            Assert.IsTrue(directoryInfo2.Is_Directory);
            Assert.IsTrue(directoryInfo2.Exists);

            Assert.IsTrue(directoryInfo.FullName == directoryInfo2.FullName);

            var newFile = new SqlFileInfo(Path.Combine(directoryFullName , "newDirectory", "newFile.txt"));
            var newFileDirectory = newFile.Directory;
            Assert.IsFalse(newFileDirectory.Exists);
            newFileDirectory.Create();
         

     
            Assert.IsTrue(newFile.Directory.Exists);

            using (var sw = new StreamWriter(newFile.Create()))
            {
                sw.WriteLine("Unit Test");
            }
            Assert.IsTrue(newFile.Exists);

      
            //TODO: add override   Directory.Delete(path, bool recursive)
            Assert.ThrowsException<SqlException>(() => newFileDirectory.Delete());


            var anotherDirectory = SqlDirectory.Create(Path.Combine(directoryFullName, "anotherDirectory"));
            Assert.IsNotNull(anotherDirectory);
            Assert.IsTrue(anotherDirectory.Exists);
            Assert.IsTrue(anotherDirectory.FullName.StartsWith(directoryFullName));

            newFile.MoveTo(Path.Combine(anotherDirectory.FullName, newFile.Name));


            newFileDirectory.Delete();
            Assert.IsFalse(newFileDirectory.Exists);



            var text = newFile.ReadAllText();
            Assert.IsTrue(text == "Unit Test\r\n");

            Directory.Move(anotherDirectory.FullName, anotherDirectory.FullName + "_v2");

            Assert.IsFalse(newFile.Exists);

            var v2 = new SqlDirectoryInfo(anotherDirectory.FullName + "_v2");
            newFile = v2.GetFiles()[0];
            Assert.IsTrue(newFile.Exists);
            Assert.IsTrue(newFile.Name == "newFile.txt");
            text = newFile.ReadAllText();
            Assert.IsTrue(text == "Unit Test\r\n");
            newFile.Delete();

            Assert.IsFalse(newFile.Exists);



            v2.Delete();
            Assert.IsFalse(v2.Exists);

            fileInfo.Directory.Delete(true);
            Assert.IsFalse(fileInfo.Directory.Exists);
            Assert.IsFalse(fileInfo.Exists);


        }
    }
}