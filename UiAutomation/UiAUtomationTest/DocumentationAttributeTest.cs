using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAutomationTest
{
    [TestClass]
    public class DocumentationAttributeTest
    {
        [TestMethod, TestCategory("Unit")]
        public void DocumentationAttributeTest1()
        {
            Assert.AreEqual("test", new DocumentationAttribute("test").Message);
        }
    }
}