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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest;

[TestClass]
public class ClassicApplicationTest
{
    [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(Win32Exception), "The system cannot find the file specified")]
    public void ClassicApplicationConstructorNonexistingFileRaisesException()
    {
        _ = new ClassicApplication("nonexisting.exe", null, null);
    }

    [TestMethod, TestCategory("DemoApp"), DeploymentItem("WpfDemoApp.exe"), DeploymentItem("WpfDemoApp.exe.config")]
    public void ClassicApplicationConstructorTest1()
    {
        var app = new ClassicApplication("WpfDemoApp.exe", null, null);
        app.WaitForInputIdle();
        Assert.IsTrue(app.WaitForMainWindow());
        Assert.IsTrue(app.IsActive, "App is active");
        Assert.IsNotNull(app.MainWindowHandle, "Handle not null");
        ExtensionFunctions.TimeoutInMilliseconds = 2000;
        Assert.IsTrue(app.Exit(false), "Exit");
    }
}