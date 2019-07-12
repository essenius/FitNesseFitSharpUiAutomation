using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAUtomationTest
{
    [TestClass]
    public class GridITemTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"),
         DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "GridItem", DataAccessMethod.Sequential), DeploymentItem("UiAutomationTest\\TestData.xml")]
        public void GridItemParseTest()
        {
            var input = TestContext.DataRow["input"].ToString();
            var output = TestContext.DataRow["output"].ToString();
            var item = GridItem.Parse(input);
            Assert.AreEqual(output, item.ToString());
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void GridItremNullArgumentTest()
        {
            var _ = new GridItem(null);
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void GridItemWrongArgumentTest()
        {
            var _ = new GridItem("bogus");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void GridItemNoColumnMatchTest()
        {
            var _ = new GridItem("col x");
        }


    }
}
