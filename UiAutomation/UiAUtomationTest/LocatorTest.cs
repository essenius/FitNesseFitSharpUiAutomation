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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class LocatorTest
    {
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
        public void LocatorConstructorTest()
        {
            var locator = new Locator("abc:def");
            Assert.AreEqual("abc", locator.Method, "Method OK");
            Assert.AreEqual("def", locator.Criterion, "Locator OK");
            locator = new Locator("def");
            Assert.AreEqual("Name", locator.Method, "Method OK");
            Assert.AreEqual("def", locator.Criterion, "Locator OK");
        }

        [TestInitialize]
        public void LocatorInitialize() => Locator.DefaultConditionType = "Name";

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