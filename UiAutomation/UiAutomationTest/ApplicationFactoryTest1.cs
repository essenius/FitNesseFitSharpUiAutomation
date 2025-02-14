// Copyright 2017-2025 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest;

[TestClass]
public class ApplicationFactoryTest
{
    [TestMethod, TestCategory("DefaultApps")]
    public void ApplicationFactoryStartClassicTest()
    {
        // we need a classic Windows app for this test. Notepad is now UWP, and Wordpad disappeared in Win 11 2024H2. Winver is still there.
        const string winVerApp = @"winver.exe";

    var app = ApplicationFactory.Start(winVerApp, null);
        app.WaitForInputIdle();
        Assert.IsInstanceOfType(app, typeof(ClassicApplication), "app is classic");
        var app1 = ApplicationFactory.AttachToProcess(app.ProcessId);
        Assert.IsInstanceOfType(app1, typeof(ClassicApplication), "app1 is classic");
        Assert.AreEqual(app.ProcessId, app1.ProcessId, "Process IDs equal");
        ExtensionFunctions.TimeoutInMilliseconds = 1000;
        Assert.IsTrue(app.Exit(true), "Exit");
        Assert.IsFalse(app.IsActive, "App is not active");
        Assert.IsFalse(app1.IsActive, "App1 is not active");
    }

    [TestMethod, TestCategory("DefaultApps")]
    public void ApplicationFactoryStartNonExistingAppTest()
    {
        var app = ApplicationFactory.Start(@"nonexisting", null);
        Assert.IsNull(app);
    }

    [TestMethod, TestCategory("DefaultApps")]
    public void ApplicationFactoryStartUwpTest()
    {
        var app = ApplicationFactory.Start(@"windows.immersivecontrolpanel_cw5n1h2txyewy", null);
        app.WaitForInputIdle();
        Assert.IsInstanceOfType(app, typeof(UwpApplication));
        var app1 = ApplicationFactory.AttachToProcess(app.ProcessId);
        Debug.WriteLine(app1.ProcessId);
        Assert.IsInstanceOfType(app1, typeof(UwpApplication));
        Assert.AreEqual(app.ProcessId, app1.ProcessId);
        ExtensionFunctions.TimeoutInMilliseconds = 3000;
        Assert.IsTrue(app.Exit(true), "Exit succeeds");
        Assert.IsFalse(app.IsActive, "App is not active");
        Assert.IsFalse(app1.IsActive, "App1 is not active");
        Assert.IsNull(app1.WindowControl, "WindowControl for exited app is null");
        ExtensionFunctions.TimeoutInMilliseconds = 1000;
        Assert.IsTrue(app1.Exit(false), "Exiting a second time should work");
    }

    [TestMethod, TestCategory("Unit")]
    public void UwpApplicationStartNonExistingAppTest()
    {
        var app = new UwpApplication(@"nonexisting", string.Empty);
        Assert.IsFalse(app.IsActive, "Non-started app is not active");
        app.WaitForInputIdle(); // should succeed
        ExtensionFunctions.TimeoutInMilliseconds = 1000;
        Assert.IsTrue(app.Exit(false), "Exiting a non-started app succeeds");
        Assert.IsNull(app.WindowControl, "WindowControl for non started app is null");
    }
}
