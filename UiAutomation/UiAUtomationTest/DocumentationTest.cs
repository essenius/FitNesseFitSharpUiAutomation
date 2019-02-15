using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAUtomationTest
{
    [TestClass]
    public class DocumentationTest
    {
        [TestMethod, TestCategory("Unit")]
        public void UiAutomationFixtureDocumentationTest()
        {
            var doc = UiAutomationFixture.FixtureDocumentation;
            Assert.IsTrue(doc.Count > 0, "There are documentation entries");
            Assert.IsTrue(doc.ContainsKey(nameof(UiAutomationFixture.ClickControl)), "ClickControl has an entry");
        }
        [TestMethod, TestCategory("Unit")]
        public void ExtractGridDocumentationTest()
        {
            var doc = ExtractGrid.FixtureDocumentation;
            Assert.IsTrue(doc.Count > 0, "There are documentation entries");
            Assert.IsTrue(doc.ContainsKey(nameof(ExtractGrid.Query)), "Query has an entry");
        }

    }
}
