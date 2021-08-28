// Copyright 2013-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using interop.UIAutomationCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class LocatorTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void LocatorByFunctionTest()
        {
            var conditionTypeList = new[] {"Name", "id", "ControlType", "CAPTION", "PrOcEsSiD"};
            foreach (var conditionType in conditionTypeList)
            {
                var locator = new Locator(conditionType + ":abc");
                Assert.AreEqual(locator.Method, conditionType);
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void LocatorConditionTypeTest()
        {
            var locator = new Locator("Id:MyId");
            Assert.AreEqual(UIA_PropertyIds.UIA_AutomationIdPropertyId, locator.ConditionType, "ConditionType match for Id");
            Assert.AreEqual("MyId", locator.ConditionValue, "ConditionValue match for Id");
            locator = new Locator("ControlType:RadioButton");
            Assert.AreEqual(UIA_PropertyIds.UIA_ControlTypePropertyId, locator.ConditionType, "ConditionType match for ControlType");
            Assert.AreEqual(UIA_ControlTypeIds.UIA_RadioButtonControlTypeId, locator.ConditionValue, "ConditionValue match for ControlType");
            locator = new Locator("IsEnabled:false");
            Assert.AreEqual(UIA_PropertyIds.UIA_IsEnabledPropertyId, locator.ConditionType, "ConditionType match for IsEnabled");
            Assert.AreEqual(false, locator.ConditionValue, "ConditionValue match for IsEnabled");
            locator = new Locator("ProcessId:123");
            Assert.AreEqual(UIA_PropertyIds.UIA_ProcessIdPropertyId, locator.ConditionType, "ConditionType match for ProcessId");
            Assert.AreEqual(123, locator.ConditionValue, "ConditionValue match for ProcessId");
        }

        [DataTestMethod, TestCategory("Unit")]
        [DataRow("abc:def [1,2]", "abc", "def", "1,2")]
        [DataRow("abc: def", "abc", "def", "")]
        [DataRow("def", "Name", "def", "")]
        [DataRow("def [ 1 ]", "Name", "def", " 1 ")]

        public void LocatorConstructorTest(string input, string method, string criterion, string gridItem)
        {
            /*var input = TestContext.DataRow["input"].ToString();
            var method = TestContext.DataRow["method"].ToString();
            var criterion = TestContext.DataRow["criterion"].ToString();
            var gridItem = TestContext.DataRow["griditem"].ToString(); */

            var locator = new Locator(input);
            Assert.AreEqual(method, locator.Method, "Method OK");
            Assert.AreEqual(criterion, locator.Criterion, "Locator OK");
            Assert.AreEqual(gridItem, locator.GridItem);
        }

        [TestMethod, TestCategory("Unit")]
        public void LocatorGridSpecTest()
        {
            Assert.AreEqual("Name", Locator.DefaultConditionType, "Default is Name");

            var conditionList = new[] {"Name", "Id", "ControlType", "Caption", "ProcessId"};
            foreach (var condition in conditionList)
            {
                Locator.DefaultConditionType = condition;
                Assert.AreEqual(condition, Locator.DefaultConditionType, "Setting to {0}", condition);
            }

            Locator.DefaultConditionType = "bogusValue";
            Assert.AreNotEqual("bogusValue", Locator.DefaultConditionType);
        }

        [TestInitialize]
        public void LocatorInitialize() => Locator.DefaultConditionType = "Name";

        [TestMethod, TestCategory("Unit")]
        public void LocatorIsWindowTest()
        {
            var locator = new Locator("Id:MyId");
            Assert.IsFalse(locator.IsWindowSearch, "Id");
            locator = new Locator("ControlType:Window");
            Assert.IsTrue(locator.IsWindowSearch, "ControlType Window");
            locator = new Locator("ControlType:Text");
            Assert.IsFalse(locator.IsWindowSearch, "ControlType Text");
            locator = new Locator("Id:Window");
            Assert.IsFalse(locator.IsWindowSearch, "Id Window");
        }

        [TestMethod, TestCategory("Unit")]
        public void LocatorSetDefaultConditionTypeTest()
        {
            Assert.AreEqual("Name", Locator.DefaultConditionType, "Default is Name");

            var conditionList = new[] {"Name", "Id", "ControlType", "Caption", "ProcessId"};
            foreach (var condition in conditionList)
            {
                Locator.DefaultConditionType = condition;
                Assert.AreEqual(condition, Locator.DefaultConditionType, "Setting to {0}", condition);
            }

            Locator.DefaultConditionType = "bogusValue";
            Assert.AreNotEqual("bogusValue", Locator.DefaultConditionType);
        }
    }
}
