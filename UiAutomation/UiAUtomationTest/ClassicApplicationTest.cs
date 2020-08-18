using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class ClassicApplicationTest
    {
        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(Win32Exception), "The system cannot find the file specified")]
        public void ClassicApplicationConstructorNonexistingFileRaisesException()
        {
            var _ = new ClassicApplication("nonexisting.exe", null, null);
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void ClassicApplicationConstructorTest1()
        {
            var app = new ClassicApplication("notepad.exe", null, null);
            app.WaitForInputIdle();
            Assert.IsNotNull(app.MainWindowHandle);
            ExtensionFunctions.TimeoutInMilliseconds = 1000;
            Assert.IsTrue(app.Exit(false));
        }
    }
}
