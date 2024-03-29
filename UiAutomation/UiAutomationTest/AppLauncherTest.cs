﻿// Copyright 2017-2023 Rik Essenius
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
using UiAutomation;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class AppLauncherTest
    {
        [TestMethod]
        [TestCategory("DefaultApps")]
        public void AppLauncherResolveTest()
        {
            using (var launcher1 = new AppLauncher("Microsoft.NET.Native.Runtime.2.2_8wekyb3d8bbwe"))
            {
                Assert.IsTrue(launcher1.FullName.Contains("_x64__"));
            }
            using var launcher2 = new AppLauncher("Windows.PrintDialog_cw5n1h2txyewy");
            Assert.IsTrue(launcher2.FullName.Contains("_neutral_neutral_"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AppLauncherTest2()
        {
            using var launcher = new AppLauncher("bogus");
            Assert.IsFalse(launcher.Exists);
        }

        [TestMethod]
        [TestCategory("DefaultApps")]
        public void AppLauncherUwpAppTest()
        {
            var fixture = new UiAutomationFixture();
            UiAutomationFixture.TimeoutSeconds = 2;
            Assert.IsTrue(
                fixture.StartApplicationWithArguments(@"windows.immersivecontrolpanel_cw5n1h2txyewy", null), "App started");
            Assert.IsTrue(fixture.IsUwpApp(), "Is UWP App");
            // Switch to parent as that contains the close button. Elements on child windows are found too.
            // UWP apps have a container called Application Frame Host.
            // So they are not direct children of the desktop, unlike "normal" applications. 
            // That means that you need to use "descendants" rather than "children" in findfirst,
            // which is slow especially if a control is not found

            // TODO: You need to be very careful here. Sometimes the wrong close button gets pushed.
            // e.g. once I had an existing Calculator instance running, and the process closed 
            // Edge instead of the new calculator window. Still need to find out how to prevent that.
            // For now the workaround is ensuring that there are no other instances of your app running.

            Assert.IsTrue(fixture.SwitchToParentWindow(), "Switch to parent.");

            Assert.IsTrue(fixture.ClickControl("ControlType:ListItem && name:System"), "click system");
            Assert.IsTrue(fixture.WaitForControl("id:PagesListView"));
            fixture.SetValueOfControlTo("id:TextBox", "ab");
            Assert.IsTrue(fixture.WaitForControlAndClick("Name:About your PC"));
            // This is needed. If you don't do it, the process gets into a locked state.
            Assert.IsTrue(fixture.WaitForControl("ControlType:Text && name:About"), "Wait for About text");
            // The About title comes earlier than the rest of the page, so wait for the control we want to examine
            Assert.IsTrue(fixture.WaitForControl("id:SystemSettings_PCSystem_ProcessorStatus_DescriptionTextBlock"), "Wait for processor");
            var version = fixture.ValueOfControl("id:SystemSettings_PCSystem_ProcessorStatus_ValueTextBlock");
            Debug.Print("Processor from settings: " + version);
            Assert.IsFalse(string.IsNullOrEmpty(version), "Processor is not empty");
            Assert.IsTrue(fixture.ClickControl("name:Close Settings"), "Press Close Settings");
        }
    }
}
