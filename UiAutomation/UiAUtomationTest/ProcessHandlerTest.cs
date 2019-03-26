using System;
using System.Diagnostics;
using interop.UIAutomationCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;
using UiAutomationTest;


namespace UiAUtomationTest
{
    [TestClass]
    public class ProcessHandlerTest
    {
        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException), "Could not understand process condition id:1")]
        public void ProcessHandlerTest1()
        {
            var handler = new ProcessHandler("id:1");
        }

        [TestMethod, TestCategory("Experiments")]
        public void ProcessHandlerDiagsTest()
        {
            var automation = new CUIAutomation();
            var root = automation.GetRootElement();
            var elements = root.FindAll(TreeScope.TreeScope_Children, automation.CreateTrueCondition());
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements.GetElement(i);
                Debug.Print(element.CurrentClassName + ";" + element.CurrentName + ";" + element.CurrentProcessId + ";" + element.CurrentFrameworkId + ";" + element.CurrentNativeWindowHandle);
            }
            Debug.Print("---");
            var frameElements = root.FindAll(TreeScope.TreeScope_Children,
                automation.CreatePropertyConditionEx(
                    UIA_PropertyIds.UIA_ClassNamePropertyId, "ApplicationFrameWindow",
                    PropertyConditionFlags.PropertyConditionFlags_IgnoreCase));
            if (frameElements.Length == 0)
            {
                Debug.Print("No UWP apps");
                return;
            }
            for (var i = 0; i < frameElements.Length; i++)
            {
                var frameElement = frameElements.GetElement(i);

                var subElements = frameElement.FindAll(TreeScope.TreeScope_Children, automation.CreateTrueCondition());
                for (var j = 0; j < subElements.Length; j++)
                {
                    var subElement = subElements.GetElement(j);
                    Debug.Print(subElement.CurrentClassName + ";" + subElement.CurrentName + ";" + subElement.CurrentProcessId + ";" +
                                subElement.CurrentFrameworkId + ";" + subElement.CurrentNativeWindowHandle);
                }
                Debug.Print("--");
            }
        }
    }
}
