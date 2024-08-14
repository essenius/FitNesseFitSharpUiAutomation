// Copyright 2013-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class FileSystemTest
    {
        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(FileNotFoundException),
             "Could not find file [calc.exe] in the Environment Path"
         )]
        public void FileSystemConstructorTest()
        {
            var fileSystem = new FileSystem(null);
            fileSystem.FindExecutable("calc.exe");
        }

        [TestMethod, TestCategory("Unit")]
        public void FileSystemFindExecutableTest()
        {
            var fileSystem = new FileSystem();
            Assert.AreEqual(
                "C:\\Windows\\System32\\calc.exe",
                fileSystem.FindExecutable("Calc.exe"),
                "Found in env path"
            );
            Assert.AreEqual(
                "C:\\Windows\\System32\\calc.exe",
                fileSystem.FindExecutable("%windir%\\System32\\Calc.exe"),
                "Path with variable, found"
            );
            Assert.IsNotNull(fileSystem.FindExecutable("UiAutomation.dll"), "Current folder, not in path, found)");
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(FileNotFoundException),
             "Could not find file with path [System32\\calc.exe]"
         )]
        public void FileSystemFindExecutableTrowTest1() => new FileSystem().FindExecutable("System32\\calc.exe");

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(FileNotFoundException),
             "Could not find file [nonexisting.exe] in the Environment Path"
         )]
        public void FileSystemFindExecutableTrowTest2() => new FileSystem().FindExecutable("nonexisting.exe");
    }
}
