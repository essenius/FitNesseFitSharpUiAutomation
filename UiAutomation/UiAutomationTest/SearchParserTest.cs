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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest

{
    [TestClass]
    public class SearchParserTest
    {
        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("abc:def && ghi:jkl", 2, new object[] { "abc", "ghi" }, new object[] { "def", "jkl" },
            new object[] { })]
        [DataRow("  ab   &&  cd   ", 2, new object[] { "Name", "Name" }, new object[] { "ab", "cd" }, new object[] { })]
        [DataRow("ab&&cd", 1, new object[] { "Name" }, new object[] { "ab&&cd" }, new object[] { })]
        [DataRow("ab && cd:ef && gh:ij", 3, new object[] { "Name", "cd", "gh" }, new object[] { "ab", "ef", "ij" },
            new object[] { "", "", "" })]
        [DataRow("&&", 1, new object[] { "Name" }, new object[] { "&&" }, new object[] { })]
        [DataRow(" && ", 2, new object[] { "Name", "Name" }, new object[] { "", "" }, new object[] { })]
        [DataRow("A & B", 1, new object[] { "Name" }, new object[] { "A & B" }, new object[] { })]
        [DataRow("A & B && C & D", 2, new object[] { "Name", "Name" }, new object[] { "A & B", "C & D" },
            new object[] { })]
        [DataRow("  abc  ", 1, new object[] { "Name" }, new object[] { "abc" }, new object[] { })]
        [DataRow("abc:def", 1, new object[] { "abc" }, new object[] { "def" }, new object[] { })]
        [DataRow("abc : def", 1, new object[] { "abc" }, new object[] { "def" }, new object[] { })]
        [DataRow("abc:def:ghi", 1, new object[] { "abc" }, new object[] { "def:ghi" }, new object[] { })]
        [DataRow("abc : def:ghi", 1, new object[] { "abc" }, new object[] { "def:ghi" }, new object[] { })]
        [DataRow("abc : def : ghi", 1, new object[] { "abc" }, new object[] { "def : ghi" }, new object[] { })]
        [DataRow("abc:def[row 1, column 1]", 1, new object[] { "abc" }, new object[] { "def" },
            new object[] { "row 1, column 1" })]
        [DataRow(":abc", 1, new object[] { "" }, new object[] { "abc" }, new object[] { })]
        [DataRow("abc:", 1, new object[] { "abc" }, new object[] { "" }, new object[] { })]
        [DataRow(":", 1, new object[] { "" }, new object[] { "" }, new object[] { })]
        [DataRow("", 1, new object[] { "Name" }, new object[] { "" }, new object[] { })]
        public void SearchParserAndCriteriaTest(string input, int expectedCriterionCount, object[] expectedMethod,
            object[] expectedLocator, object[] expectedGridItem)
        {
            Locator.DefaultConditionType = "Name";
            var searchParser = new SearchParser(input);

            Assert.AreEqual(expectedCriterionCount, searchParser.Locators.Count, "Locator count OK");
            for (var i = 0; i < expectedCriterionCount; i++)
            {
                Assert.AreEqual(expectedMethod[i], searchParser.Locators[i].Method, "Methods OK");
                Assert.AreEqual(expectedLocator[i], searchParser.Locators[i].Criterion, "Locator OK");
                if (expectedGridItem.Length > i)
                {
                    Assert.AreEqual(expectedGridItem[i], searchParser.Locators[i].GridItem, "GridItem OK");
                }
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SearchParserCheckValidProcessCondition()
        {
            Assert.IsTrue(new SearchParser("ProcessId:123").IsValidProcessCondition());
            Assert.IsTrue(new SearchParser("Name:Calculator").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("ControlType:abc").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("id:123").IsValidProcessCondition());
            Assert.IsFalse(new SearchParser("ProcessId:123 && id:abc").IsValidProcessCondition());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchParserFindElement1NullTest()
        {
            var _ = new SearchParser(null);
        }
    }
}
