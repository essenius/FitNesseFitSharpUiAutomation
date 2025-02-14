// Copyright 2013-2025 Rik Essenius
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
using System.Drawing;
using System.Text.RegularExpressions;
using ImageHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAutomationTest;

[TestClass]
public class ScreenCaptureTest
{
    private UiAutomationFixture _fixture;

    [TestCleanup]
    public void Cleanup() => _fixture.ForcedCloseApplication();

    [TestInitialize]
    public void Init()
    {
            _fixture = new UiAutomationFixture();
            Assert.IsTrue(_fixture.StartApplication(FixtureTest.SystemInfoApp), "System Info started");
            Assert.IsTrue(_fixture.WaitForControl(@"System Summary"), "Found system summary");
            _fixture.ResizeWindowTo(new Coordinate(800, 600));
    }

    [TestMethod, TestCategory("DefaultApps")]
    public void ScreenCaptureTakeTest()
    {
            var image = Snapshot.CaptureScreen(new Rectangle(0, 0, 2, 1));
            Assert.IsTrue(image.ToString().StartsWith("Image", StringComparison.Ordinal));
            Assert.IsTrue(image.ToString().EndsWith("(2 x 1)", StringComparison.Ordinal));
            Assert.AreEqual("image/jpeg", image.MimeType);
            Assert.IsTrue(_fixture.ClickControl("Help"));
            Assert.IsTrue(_fixture.WaitForControlAndClick("About System Info..."));
            _fixture.WaitForControl("Name:About System Information && ControlType:Window");
            var screenshot = _fixture.SnapshotObjectOfControl("Name:About System Information && ControlType:Window");
            Assert.IsTrue(screenshot.ToString().StartsWith("Image", StringComparison.Ordinal), "Starts With Image");
            Assert.IsTrue(Regex.IsMatch(screenshot.ToString(), @".+\(\d+ x \d+\)$"), "Ends With (n x m)");
            var screenshotRendering = _fixture.SnapshotOfControl("Name:About System Information && ControlType:Window");
            Assert.IsTrue(
                screenshotRendering.StartsWith("<img src=\"data:image/jpeg;base64,", StringComparison.Ordinal), "starts with <img src"
            );
            Assert.IsTrue(_fixture.ClickControl("Name:OK"));
        }
}