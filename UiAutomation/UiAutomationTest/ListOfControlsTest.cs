// Copyright 2019-2024 Rik Essenius
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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAutomationTest
{
    [TestClass]
    public class ListOfControlsTest
    {
        [TestMethod, TestCategory("DefaultApps")]
        public void ListOfControlsQueryTest()
        {
            var listOfControls = new ListOfControls(null, "ControlType:Window");
            var result = listOfControls.Query();
            var firstRow = result[0] as Collection<object>;
            Assert.IsNotNull(firstRow);
            var cell1 = firstRow[0] as Collection<string>;
            Assert.IsNotNull(cell1);
            Assert.AreEqual("Automation Id", cell1[0]);
            foreach (var row in result)
            {
                var rowCollection = row as Collection<object>;
                Assert.IsNotNull(rowCollection);
                foreach (var cell in rowCollection)
                {
                    var cellCollection = cell as Collection<string>;
                    Assert.IsNotNull(cellCollection);
                    Console.Write($@"{cellCollection[0]}:{cellCollection[1]}; ");
                }

                Console.WriteLine();
            }
        }

        [TestMethod, TestCategory("DefaultApps")]
        public void ListOfControlsTableTest()
        {
            var fixture = new UiAutomationFixture();
            Assert.IsTrue(fixture.StartApplication(FixtureTest.WordPadPath), "Started application");
            
            var processId = fixture.ApplicationProcessId;
            var listOfControls = new ListOfControls(processId, "ProcessId:" + processId);
            var result = listOfControls.DoTable();
            var headerRow = result[0] as Collection<string>;
            Assert.IsNotNull(headerRow);
            Assert.AreEqual("report:Name", headerRow[1], "heaaderRow[1] accurate");
            var controls = result.Skip(1).ToList();
            Assert.IsTrue(controls.Any());
            foreach (Collection<string> row in controls)
            {
                foreach (var cell in row)
                {
                    Console.Write($@"{cell}; ");
                }

                Console.WriteLine();
            }

            var emptyList = new ListOfControls(processId, "id:q");
            var emptyResult = emptyList.Query();
            Assert.IsNull(emptyResult, "empty result");
            fixture.ForcedCloseApplication();
        }
    }
}
