// Copyright 2019-2023 Rik Essenius
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
using UiAutomation;

namespace UiAUtomationTest
{
    [TestClass]
    public class GridITemTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void GridItemNoColumnMatchTest()
        {
            var _ = new GridItem("col x");
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow(" row 10 , column   20 ", "row 10, column 20")]
        [DataRow("12,34", "row 12, column 34")]
        [DataRow("  56 , 7  ", "row 56, column 7")]
        [DataRow("row 8", "row 8")]
        [DataRow("col9", "column 9")]
        [DataRow("column30, row40", "row 40, column 30")]
        public void GridItemParseTest(string input, string output)
        {
            var item = GridItem.Parse(input);
            Assert.AreEqual(output, item.ToString());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void GridItemWrongArgumentTest()
        {
            var _ = new GridItem("bogus");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ArgumentException))]
        public void GridItremNullArgumentTest()
        {
            var _ = new GridItem(null);
        }
    }
}
