using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;
using UiAutomation.Patterns;

namespace UiAUtomationTest
{
    [TestClass]
    public class PatternTest
    {
        [TestMethod, TestCategory("Experiments")]
        public void PatternTest1()
        {
            var fixture = new UiAutomationFixture();
            try
            {
                UiAutomationFixture.TimeoutSeconds = 1;
                UiAutomationFixture.SearchBy("Name");
                Assert.IsTrue(fixture.StartApplication("notepad.exe"), "Notepad started");
                var control = fixture.GetControl("classname:edit");
                var pattern = new LegacyIAccessiblePattern(control.AutomationElement);
                Assert.AreEqual(SetResult.Success, pattern.TrySet("hello"));
                Assert.IsTrue(pattern.TryGet(out var content));
                Assert.AreEqual("hello", content);
            }
            finally
            {
                fixture.ForcedCloseApplication();
            }
        }
    }
}
