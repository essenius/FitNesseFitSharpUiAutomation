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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using interop.UIAutomationCore;
using UiAutomation.Model;

namespace UiAutomation
{
    public class ExtractGrid
    {
        private readonly string _locator;

        /// <summary>
        ///     Import the table search criteria to identify the table we are interested in (XPath format).
        /// </summary>
        /// <param name="gridLocator">The locator (method:criterion) to find the grid control.</param>
        public ExtractGrid(string gridLocator) => _locator = gridLocator;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FixtureExplorer")]
        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Extract cell values from a grid control (Query table interface)"},
            {nameof(Query), "Executes the query to extract cell values from a grid control"}
        };

        /// <summary>
        ///     Executes the query to extract cell values from a grid control
        /// </summary>
        /// <returns>
        ///     the table in a collection of rows, where each row is a collection of key/value pairs,
        ///     or null if the control was not found or does not support the grid pattern
        /// </returns>
        public Collection<object> Query()
        {
            // we are not looking for a window here, so no need for the Window parameter
            var control = Control.FindControl(_locator, null);
            if (!(control?.AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId)
                is IUIAutomationGridPattern gridPattern))
            {
                return null;
            }

            var headerCollection = new Collection<string>();
            var headerContainer = new Control(control, SearchType.Deep, "ControlType:Header");
            headerContainer.FindControl();
            var headers = headerContainer.FindAllElements("ControlType:HeaderItem");
            foreach (var columnHeader in headers)
            {
                headerCollection.Add(columnHeader.GetCurrentPropertyValue(UIA_PropertyIds.UIA_NamePropertyId).ToString());
            }

            var rowCollection = new Collection<object>();

            for (var row = 0; row < gridPattern.CurrentRowCount; row++)
            {
                var cellCollection = new Collection<object>();
                for (var column = 0; column < gridPattern.CurrentColumnCount; column++)
                {
                    if (headerCollection.Count <= column)
                    {
                        headerCollection.Add("Column " + (column + 1));
                    }

                    var cell = gridPattern.GetItem(row, column);
                    cellCollection.Add(new Collection<object> {headerCollection[column], Control.GetValue(cell)});
                }

                rowCollection.Add(cellCollection);
            }

            return rowCollection;
        }
    }
}