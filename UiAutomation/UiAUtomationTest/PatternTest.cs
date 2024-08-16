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

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;
using UiAutomation.Patterns;

namespace UiAutomationTest;

[TestClass]
public class PatternTest
{
    [TestMethod, TestCategory("DefaultApps")]
    public void LegacySetGetTest()
    {
        var fixture = new UiAutomationFixture();
        try
        {
            UiAutomationFixture.TimeoutSeconds = 3;
            UiAutomationFixture.SearchBy("Name");
            if (!fixture.SwitchToProcess("name:Notepad"))
            {
                fixture.NoAutomaticSwitchToStartedApplication();
                Assert.IsTrue(fixture.StartApplication(@"Microsoft.WindowsNotepad_8wekyb3d8bbwe"), "Notepad started");
                while (fixture.ApplicationIsActive) Thread.Sleep(100);
                Assert.IsTrue(fixture.SwitchToProcess("name:Notepad"), "Switch succeeded");
                Assert.IsTrue(fixture.IsUwpApp(), "Is UWP App");
            }

            // Assert.IsTrue(fixture.SwitchToParentWindow(), "Switch to parent.");
            fixture.PressKey("^n");
            fixture.WaitForControl("Untitled");
            var control = fixture.GetControl("classname:RichEditD2DPT");
            Assert.IsNotNull(control, "edit control found");
            var pattern = new LegacyIAccessiblePattern(control.AutomationElement);
            Assert.AreEqual(SetResult.Success, pattern.TrySet("hello"));
            Assert.IsTrue(pattern.TryGet(out var content));
            Assert.AreEqual("hello", content);
            fixture.PressKeys("^a{DELETE}^w");
        }
        finally
        {
            fixture.ForcedCloseApplication();
        }
    }

    private static void AssertGetFalse(IPattern pattern)
    {
        var result = pattern.TryGet(out var content);
        Assert.IsFalse(result);
        Assert.IsNull(content);
    }

    private static void AssertSetNotApplicable(IPattern pattern)
    {
        var result = pattern.TrySet("x");
        Assert.AreEqual(SetResult.NotApplicable, result);
    }

    [TestMethod, TestCategory("Unit")]
    public void LegacyTryGetNullTest()
    {
        var elementMock = new IUIAutomationElementMock { CurrentIsKeyboardFocusable = 0 };
        var pattern = new LegacyIAccessiblePattern(elementMock);
        AssertGetFalse(pattern);
    }

    [TestMethod, TestCategory("Unit")]
    public void LegacyTrySetNotApplicableTest()
    {
        var elementMock = new IUIAutomationElementMock { CurrentIsKeyboardFocusable = 0 };
        var pattern = new LegacyIAccessiblePattern(elementMock);
        AssertSetNotApplicable(pattern);
    }

    [TestMethod, TestCategory("Unit")]
    public void TextTrySetNotApplicableTest()
    {
        var elementMock = new IUIAutomationElementMock();
        var pattern = new TextPattern(elementMock);
        AssertSetNotApplicable(pattern);
    }

    [TestMethod, TestCategory("Unit")]
    public void ToggleTrySetNotApplicableTest()
    {
        var elementMock = new IUIAutomationElementMock();
        var pattern = new TogglePattern(elementMock);
        AssertSetNotApplicable(pattern);
    }

}
