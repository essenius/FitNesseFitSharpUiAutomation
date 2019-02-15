// Copyright 2017-2019 Rik Essenius
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

namespace UiAutomationTest
{
    [TestClass]
    public class ApplicationFactoryTest
    {
        [TestMethod, TestCategory("DefaultApps")]
        public void ApplicationFactoryStartClassicTest()
        {
            var app = ApplicationFactory.Start("notepad.exe", null);
            app.WaitForInputIdle();
            Assert.IsInstanceOfType(app, typeof(ClassicApplication));
            var app1 = ApplicationFactory.AttachToProcess(app.ProcessId);
            Assert.IsInstanceOfType(app1, typeof(ClassicApplication));
            Assert.AreEqual(app.ProcessId, app1.ProcessId);
            Assert.IsTrue(app.Exit(false));
            Assert.IsFalse(app.IsActive, "App is not active");
            Assert.IsFalse(app1.IsActive, "App1 is not active");
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void ApplicationFactoryStartNonexistingAppTest()
        {
            var app = ApplicationFactory.Start("nonexisting", null);
            Assert.IsNull(app);
        }

        // not working yet. [TestMethod, TestCategory("DefaultApps")]
        public void ApplicationFactoryStartUrlTest()
        {
            var app = ApplicationFactory.Start("ms-settings:", null);
            Assert.IsInstanceOfType(app, typeof(ClassicApplication));
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
            //var app2 = factory.AttachToProcessByName("SystemSettings");
            Assert.IsTrue(app.Exit(false));
            Assert.IsFalse(app.IsActive, "App is not active");
            Assert.IsFalse(app1.IsActive, "App1 is not active");
            Assert.IsNull(app1.WindowControl, "WindowControl for exited app is null");
            Assert.IsTrue(app1.Exit(false), "Exiting a second time should work");
        }

        [TestMethod, TestCategory("Unit")]
        public void UwpApplicationStartNonexistingAppTest()
        {
            var app = new UwpApplication("nonexisting", string.Empty);
            Assert.IsFalse(app.IsActive, "Non-started app is not active");
            app.WaitForInputIdle(); // should succeed
            Assert.IsTrue(app.Exit(false), "Exiting a non-started app succeeds");
            Assert.IsNull(app.WindowControl, "WindowControl for non started app is null");
        }
    }
}