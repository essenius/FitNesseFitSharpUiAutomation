// Copyright 2013-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class FixtureTest
    {
        private const string WordPath = @"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE";
        private UiAutomationFixture _fixture;

        [TestMethod, TestCategory("Unit")]
        public void FixtureAppUsageFailures()
        {
            var fixture = new UiAutomationFixture();
            UiAutomationFixture.SearchBy("Name");
            Assert.IsFalse(fixture.SwitchToAppWindow(), "Cannot switch to app window if not started");
            Assert.IsFalse(fixture.SwitchToParentWindow(), "cannot switch to parent if no app launched");
            Assert.IsFalse(fixture.IsUwpApp(), "IsUwpApp returns false if no app started");
#pragma warning disable 618
            Assert.IsFalse(fixture.SwitchToWindow("irrelevant"), @"Can't switch to nonexisting window");
#pragma warning restore 618
            Assert.IsFalse(fixture.SwitchToProcess("irrelevant"));
            Assert.IsFalse(fixture.MaximizeWindow(), "Can't maximize nonexisting window");
            Assert.IsFalse(fixture.MinimizeWindow(), "Can't minimize nonexisting window");
            Assert.IsFalse(fixture.MoveWindowTo(new Coordinate(10, 10)), "Can't move nonexisting window");
            Assert.IsFalse(fixture.NormalWindow(), "Can't restore nonexisting window");
            Assert.IsFalse(fixture.ResizeWindowTo(new Coordinate(100, 100)), "Can't resize nonexisting window");
            var topleft = fixture.WindowTopLeft;
            var size = fixture.WindowSize;
            Assert.AreEqual(0, size.X, "Width of nonexisting window is 0");
            Assert.AreEqual(0, size.Y, "height of nonexisting window is 0");
            Assert.AreEqual(0, topleft.X, "Row of nonexisting window is 0");
            Assert.AreEqual(0, topleft.Y, "Column of nonexisting window is 0");
        }

        [TestMethod, TestCategory("Cmd"), DeploymentItem(@"UiAutomationTest\test.cmd")]
        public void FixtureDelayedClosingCmdTest()
        {
            _fixture.NoAutomaticSwitchToStartedApplication();
            UiAutomationFixture.TimeoutSeconds = 2;
            // Another way of starting via cmd /c should work as well. Also letting it wait a second.
            Assert.IsTrue(_fixture.StartApplicationWithArguments(@"cmd.exe", "/c \"test.cmd wait 1\""), @"delayed closing Cmd");
            var processId = _fixture.ApplicationProcessId;
            Assert.IsNotNull(processId);
            Assert.IsTrue(_fixture.ApplicationIsActive);
            Assert.IsTrue(UiAutomationFixture.WaitUntilProcessEnds("ProcessId:" + processId.Value));
        }

        [TestMethod, TestCategory("Cmd"), DeploymentItem(@"UiAutomationTest\test.cmd")]
        public void FixtureImmediatelyClosingCmdTest()
        {
            _fixture.NoAutomaticSwitchToStartedApplication();
            // the command should almost immediately stop, but still slow enough to need a wait.
            Assert.IsTrue(_fixture.StartApplicationWithArguments("test.cmd", "wait 0"), @"Immediately closing Cmd");
            var processId = _fixture.ApplicationProcessId;
            Assert.IsNotNull(processId);
            Assert.IsTrue(_fixture.ApplicationIsActive);
            Assert.IsTrue(UiAutomationFixture.WaitUntilProcessEnds("ProcessId:" + processId.Value));
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void FixtureIsUwpTest()
        {
            var fixture = new UiAutomationFixture();
            fixture.StartApplication("notepad");
            Assert.IsNotNull(fixture.ApplicationProcessId);
            var process = Process.GetProcessById(fixture.ApplicationProcessId.Value);
            Assert.IsFalse(AppLauncher.IsUwpApp(process.Handle), "Notepad is not UWP");
            Assert.IsTrue(fixture.CloseApplication(), "Close notepad");
            fixture.StartApplicationWithArguments(@"windows.immersivecontrolpanel_cw5n1h2txyewy", null);
            Assert.IsNotNull(fixture.ApplicationProcessId);
            var pid = fixture.ApplicationProcessId.Value;
            process = Process.GetProcessById(pid);
            Assert.IsTrue(AppLauncher.IsUwpApp(process.Handle), "App is UWP");
            Assert.AreEqual(IntPtr.Zero, process.MainWindowHandle, "Main window handle for Uwp window is 0");
            Assert.IsTrue(fixture.SwitchToParentWindow(), "Switch to parent");
            Assert.AreNotEqual(pid, fixture.ApplicationProcessId, @"Pids are not equal");
            Assert.IsTrue(AppLauncher.IsUwpApp(process.Handle), "Parent is UWP");
            Assert.IsNotNull(fixture.ApplicationProcessId);
            process = Process.GetProcessById(fixture.ApplicationProcessId.Value);
            Assert.AreNotEqual(IntPtr.Zero, process.MainWindowHandle, "Main window handle for Uwp parent window is not 0");
            Assert.IsTrue(fixture.SwitchToProcess("ProcessId:" + pid), "Switch to child");
            Assert.IsTrue(fixture.CloseApplication(), "Close UWP app");
        }

        [TestMethod, TestCategory("Cmd")]
        public void FixtureNonExistingCmdTest()
        {
            Assert.IsFalse(_fixture.StartApplication("nonexisting.cmd"), @"nonexisting cmd file");
            Assert.IsNull(_fixture.ApplicationProcessId);
        }

        // This test is a bit flaky because the behavior of Notepad isn't entirely predictable. Sometimes e.g. it shows a horizontal scrollbar.
        [TestMethod, TestCategory("DefaultApps"),
         DeploymentItem("NotepadScreenshotNoCursor.txt"),
         DeploymentItem("NotepadScreenshotWithCursor.txt"),
         DeploymentItem("NotepadScreenshotWithCursorAndScrollbar.txt"),
         DeploymentItem("NotepadScreenshotNoCursorWithScrollbar.txt")]
        public void FixtureNotePadCheckSetValueResizeMoveAndScreenshot()
        {
            try
            {
                UiAutomationFixture.TimeoutSeconds = 1;
                UiAutomationFixture.SearchBy("Name");
                Assert.IsTrue(_fixture.CloseApplication(), "Stopping an app before it started should succeed");
                Assert.IsTrue(_fixture.ForcedCloseApplication(), "Forced stopping an app before it started should succeed");
                Assert.IsTrue(_fixture.StartApplication("notepad.exe"), "Notepad started");
                Assert.IsTrue(_fixture.SetValueOfControlTo("classname:edit",
                    "The quick brown fox jumps over the lazy dog."), "Set Text");
                Assert.IsTrue(_fixture.PressKey("^{END}{ENTER}Hello{ENTER}there"));
                Assert.AreEqual("The quick brown fox jumps over the lazy dog.\r\nHello\r\nthere",
                    _fixture.ValueOfControl(@"controltype:edit"));

                var desiredSize = new Coordinate(400, 140);
#pragma warning disable 618
                Assert.IsTrue(_fixture.ResizeWindow(desiredSize), "Resize succeeds");
#pragma warning restore 618
                Assert.AreEqual(desiredSize, _fixture.WindowSize);
                Assert.IsTrue(_fixture.MaximizeWindow());
                Assert.AreNotEqual(desiredSize, _fixture.WindowSize);
                Assert.IsTrue(_fixture.MinimizeWindow());
                Assert.IsTrue(_fixture.NormalWindow());
                Assert.AreEqual(desiredSize, _fixture.WindowSize);
                var desiredLocation = new Coordinate(200, 250);
#pragma warning disable 618
                Assert.IsTrue(_fixture.MoveWindow(desiredLocation), "Move succeeds");
#pragma warning restore 618
                Assert.AreEqual(desiredLocation, _fixture.WindowTopLeft);
                var snapshot = _fixture.WindowSnapshot(8);
                var expected1 = File.ReadAllText("NotepadScreenshotNoCursor.txt");
                var expected2 = File.ReadAllText("NotepadScreenshotWithCursor.txt");
                var expected3 = File.ReadAllText("NotepadScreenshotWithCursorAndScrollbar.txt");
                var expected4 = File.ReadAllText("NotepadScreenshotNoCursorWithScrollbar.txt");

                Console.WriteLine(snapshot);
                Assert.IsTrue(snapshot.Equals(expected1) || snapshot.Equals(expected2) ||
                              snapshot.Equals(expected3) || snapshot.Equals(expected4), "Snapshot matches");
                UiAutomationFixture.WaitSeconds(1);
                Assert.IsTrue(_fixture.ClickControl("Close"));
                Assert.IsTrue(_fixture.WaitForControl("Save"), "Wait for Save");
                Assert.IsTrue(_fixture.ClickControl("Don't Save"), "Push Don't Save");
                Thread.Sleep(500);
            }
            finally
            {
                Assert.IsTrue(_fixture.ForcedCloseApplication(), "Stopping Notepad a second time should succeed (already stopped)");
            }
        }

        [TestMethod, TestCategory("Office")]
        public void FixtureRunWord()
        {
            // Word doesn't always start a new process, but will re-use an existing process if that was already running.
            // That's why we don't use the automatic switch to the process, but switch by its process name separately
            var fixture = new UiAutomationFixture();
            UiAutomationFixture.SearchBy("Name");
            fixture.NoAutomaticSwitchToStartedApplication();
            UiAutomationFixture.TimeoutSeconds = 10;
            Assert.IsTrue(fixture.StartApplicationWithArguments(WordPath, "/w /q"), "Started Word");
            UiAutomationFixture.WaitSeconds(3);
            // todo: this fails when Word is already active when the test runs. Make more resilient
            Assert.IsTrue(fixture.SwitchToProcess(@"name:winword"), "Switched to Word");

            fixture.PressKey(@"The Quick Brown Fox Jumps Over the Lazy Dog+{HOME}");
            Assert.IsTrue(fixture.ClickControl("ControlType:Button && Name:Bold"), "Click Bold");
            Assert.IsTrue(fixture.ControlExists("Name:Heading 1"), "Heading 1 found");
            Assert.IsTrue(fixture.ClickControl("Heading 1"), "Click Heading 1");
            Assert.IsTrue(fixture.ClickControl("ControlType:Button && Name:Underline"), "Click Underline");
            Assert.IsFalse(fixture.CloseApplication(), "Close application doesn't work due to dialog");

            // This is a tricky one, as it is a control on a modal dialog.
            // It works because click has a fallback to simulate a mouseclick in the center of the control's bounding rectangle

            Assert.IsTrue(fixture.ClickControl("Name:Don't Save"), "click Don't Save");
            // Normally exiting Word can take a very long time. 10 seconds is typically not enough
            UiAutomationFixture.TimeoutSeconds = 30;
            Assert.IsTrue(UiAutomationFixture.WaitUntilProcessEnds(@"winword"), "WinWord process ends");
            UiAutomationFixture.TimeoutSeconds = 3;
            fixture.SetAutomaticSwitchToStartedApplication();
        }

        [TestMethod, TestCategory("Office")]
        public void FixtureStartAndSwitchTestOnWord2016()
        {
            UiAutomationFixture.TimeoutSeconds = 3;
            Assert.IsFalse(_fixture.SwitchToProcess(@"name:winword"), "Word not running already");
            _fixture.NoAutomaticSwitchToStartedApplication();
            UiAutomationFixture.TimeoutSeconds = 10;
            // command line switch /w opens Word with a blank page.
            Assert.IsTrue(_fixture.StartApplicationWithArguments(WordPath, "/w /q"), @"first start of Winword succeeds (no autoswitch)");
            var processId = _fixture.ApplicationProcessId;
            Assert.IsTrue(UiAutomationFixture.WaitForProcess("ProcessId:" + processId), "Wait for Word process");
            Assert.IsTrue(_fixture.SwitchToProcess("ProcessId:" + processId), "Now Word is running");
            _fixture.SetAutomaticSwitchToStartedApplication();
            Assert.IsTrue(_fixture.StartApplicationWithArguments(WordPath, "/w /q"), @"second start of Winword succeeds too (autoswitch)");
            Assert.AreNotEqual(processId, _fixture.ApplicationProcessId, "Process IDs are not equal");
            Assert.IsTrue(_fixture.ForcedCloseApplication(), "Forced close of 2nd instance succeeds");
            Assert.IsTrue(_fixture.SwitchToProcess("ProcessId:" + processId), "Can switch to first Word instance");
            Assert.IsTrue(_fixture.ForcedCloseApplication(), "Forced close 1st instance succeeds");
            Assert.IsTrue(UiAutomationFixture.WaitUntilProcessEnds(@"name:winword"), "WinWord process ends");
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureSwitchToInvalidWindowFails()
        {
            UiAutomationFixture.TimeoutSeconds = 0.7;
            Assert.IsFalse(_fixture.SwitchToProcess(@"name:nonexistingwindow"), "Switch to nonexisting process fails");
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void FixtureTestCloseAfterWait()
        {
            Assert.IsTrue(_fixture.StartApplication("notepad.exe"), "Notepad started");
            Assert.IsTrue(_fixture.SetValueOfControlTo("ControlType:edit", "text"), "Set Text");
            Assert.IsFalse(_fixture.CloseApplication(), "Closing an application showing a dialog should fail");
            Assert.IsTrue(_fixture.ClickControl("Caption:Don't Save"), "Click [Don't Save] button");
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void FixtureTestCloseByKillAfterWait()
        {
            Assert.IsTrue(_fixture.StartApplication("notepad.exe"), "Notepad started");
            Assert.IsTrue(_fixture.SetValueOfControlTo("ControlType:edit", "text"), "Set Text");
            Assert.IsTrue(_fixture.ForcedCloseApplication(), "Forced closing an application showing a dialog should succeed");
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void FixtureTestCloseDuringModalDialog()
        {
            UiAutomationFixture.SearchBy("Name");
            Assert.IsTrue(_fixture.StartApplication("notepad.exe"), "Notepad started");
            Assert.IsTrue(_fixture.ClickControl("File"), "Click File Menu");
            Assert.IsTrue(_fixture.WaitForControlAndClick("Page Setup..."), "Click Page Setup menu");
            Assert.IsTrue(_fixture.WaitForControl("Page Setup"), "Wait for Page Setup dialog");
            Assert.IsFalse(_fixture.CloseApplication(), "Closing application with a modal dialog open should fail");
            Assert.IsTrue(_fixture.ForcedCloseApplication(), "Forced closing application with a modal dialog open should succeed");
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureTestInvalidSearchBy()
        {
            Assert.IsFalse(UiAutomationFixture.SearchBy(""), "Search by empty value fails");
            Assert.IsFalse(UiAutomationFixture.SearchBy("InvalidConditionType"), "Search by wrong value fails");
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureTestMaxAndMinTimeout()
        {
            UiAutomationFixture.TimeoutSeconds = 10.0 * int.MaxValue;
            Assert.AreEqual(24 * 24 * 3600, UiAutomationFixture.TimeoutSeconds);
            UiAutomationFixture.TimeoutSeconds = 10.0 * int.MinValue;
            Assert.AreEqual(0.1, UiAutomationFixture.TimeoutSeconds);
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureTestPressKeyWithoutStartingApplication() =>
            Assert.IsFalse(_fixture.PressKey("A"), "Cannot type a letter without running an application");

        [TestMethod, TestCategory("DefaultApps")]
        public void FixtureTestRowCountOnNonGridApplication()
        {
            Assert.IsTrue(_fixture.StartApplication("notepad.exe"), "Notepad started");
            Assert.IsNull(_fixture.CellInControlContaining("Name:File", "nothing"),
                "asking row number on non-grid returns none");
            Assert.IsTrue(_fixture.CloseApplication(), "Close application");
        }

        [TestMethod, TestCategory("Unit")]
        public void FixtureTestStartInvalidApplication() =>
            Assert.IsFalse(_fixture.StartApplication("c:\\nonexisting.exe"), "Running nonexisting executable");

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void FixtureTestUnrecognizedConditionType() =>
            Assert.IsFalse(_fixture.ToggleControl("NonExistingConditionType:RandomValue"));

        [TestInitialize]
        public void SetUp()
        {
            UiAutomationFixture.TimeoutSeconds = 3;
            _fixture = new UiAutomationFixture();
        }

        [TestMethod, TestCategory("Unit")]
        public void UwpAppsAreSupportedTest()
        {
            var accessor = new PrivateType(typeof(UiAutomationFixture));
            var savedVersion = accessor.GetStaticProperty("PlatformVersion");
            accessor.SetStaticProperty("PlatformVersion", new Version(5, 0, 1000, 10));
            Assert.IsFalse(UiAutomationFixture.UwpAppsAreSupported);
            accessor.SetStaticProperty("PlatformVersion", new Version(6, 1));
            Assert.IsFalse(UiAutomationFixture.UwpAppsAreSupported);
            accessor.SetStaticProperty("PlatformVersion", new Version(6, 2, 9200, 0));
            Assert.IsTrue(UiAutomationFixture.UwpAppsAreSupported);
            accessor.SetStaticProperty("PlatformVersion", new Version(7, 0));
            Assert.IsTrue(UiAutomationFixture.UwpAppsAreSupported);
            accessor.SetStaticProperty("PlatformVersion", savedVersion);
        }
    }
}
