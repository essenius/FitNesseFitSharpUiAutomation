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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class ControlTest
    {
        [TestMethod, TestCategory("Unit")]
        public void ControlCommandsWithoutApplicationHandledOk()
        {
            var control = Control.Parse("id:non-existing element");
            Assert.AreEqual(SearchType.Deep, control.SearchType, "SearchType is deep");
            Assert.IsFalse(control.Collapse(), "Collapse fails");
            Assert.IsFalse(control.Expand(), "Expand fails");
            Assert.IsFalse(control.DoubleClick(), "DoubleClick fails");
            Assert.IsFalse(control.FindGridItem(new GridItem("1,2")), "FindGridItem fails");
            Assert.IsNull(control.FirstTextUnder(), "FirstTextUnder fails");

            Assert.IsTrue(string.IsNullOrEmpty(control.AutomationId), "Automation ID is empty");
            Assert.IsTrue(string.IsNullOrEmpty(control.Name), "Name is empty");
            Assert.AreEqual(0, control.ColumnCount, "Column Count = 0");
            Assert.AreEqual(0, control.RowCount, "RowCount = 0");
            Assert.IsNull(control.SelectedCell());

            Assert.IsFalse(control.IsEnabled(), "control is not enabled");
            Assert.IsFalse(control.IsVisible(), "control is not visible");
            Assert.AreEqual(IntPtr.Zero, control.WindowHandle, "window handle is IntPtr.Zero");
        }
    }
}