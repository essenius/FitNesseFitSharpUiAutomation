﻿// Copyright 2013-2020 Rik Essenius
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
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest

{
    [TestClass]
    public class SearchParserTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit"),
         DataSource(@"Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "SearchParser.AndCriteria", DataAccessMethod.Sequential), DeploymentItem("UiAutomationTest\\TestData.xml")]
        public void SearchParserAndCriteriaTest()
        {
            Locator.DefaultConditionType = "Name";
            var input = TestContext.DataRow["input"].ToString();
            var expectedCriterionCount = Convert.ToInt32(TestContext.DataRow["expectedCount"], CultureInfo.InvariantCulture);
            var resultList = new List<Tuple<string, string, string>>();
            for (var i = 1; i <= expectedCriterionCount; i++)
            {
                resultList.Add(new Tuple<string, string, string>(
                    TestContext.DataRow["expectedMethod" + i].ToString(),
                    TestContext.DataRow["expectedLocator" + i].ToString(),
                    TestContext.DataRow["expectedGridItem" + i]?.ToString()));
            }

            var searchParser = new SearchParser(input);

            Assert.AreEqual(resultList.Count, searchParser.Locators.Count, "Locator count OK");
            for (var i = 0; i < resultList.Count; i++)
            {
                Assert.AreEqual(resultList[i].Item1, searchParser.Locators[i].Method, "Methods OK");
                Assert.AreEqual(resultList[i].Item2, searchParser.Locators[i].Criterion, "Locator OK");
                Assert.AreEqual(resultList[i].Item3, searchParser.Locators[i].GridItem, "GridItem OK");
            }
        }

        [TestMethod, TestCategory("Unit")]
        public void SearchParserCheckValidProcessCondition()
        {
            Assert.IsTrue(new SearchParser("ProcessId:123").IsValidProcessCondition());
            Assert.IsTrue(new SearchParser("Name:Calculator").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("ControlType:abc").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("id:123").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("ProcessId:123 && id:abc").IsValidProcessCondition());
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException))]
        public void SearchParserFindElement1NullTest()
        {
            var _ = new SearchParser(null);
        }
    }
}
