// Copyright 2016-2020 Rik Essenius
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
using System.Linq;
using UiAutomation.Model;
using static System.FormattableString;

namespace UiAutomation
{
    internal class ListOfControls
    {
        private readonly int? _processId;
        private readonly string _searchCriterion;

        public ListOfControls(int? processId, string searchCriterion)
        {
            _processId = processId;
            _searchCriterion = searchCriterion;
        }

        private Collection<Collection<string>> CreateBaseResult()
        {
            List<Control> list;
            if (_processId != null)
            {
                var app = ApplicationFactory.AttachToProcess(_processId.Value);
                app.WaitForInputIdle();
                var control = app.WindowControl;
                // This is not only necessary to potentially wait, but also to populate the parent property in the control.
                control.WaitTillFound();
                list = control.DescendantControls(_searchCriterion).ToList();
            }
            else
            {
                list = Control.RootChildControls(_searchCriterion).ToList();
            }

            var rows = new Collection<Collection<string>> {new Collection<string> {"Automation Id", "Name", "Value", "Location"}};
            foreach (var entry in list)
            {
                var position = Mouse.AbsolutePosition(entry.AutomationElement);
                var row = new Collection<string> {entry.AutomationId, entry.Name, entry.Value, Invariant($"x:{position.X}, y:{position.Y}")};
                rows.Add(row);
            }
            return rows;
        }

        /// <summary>
        ///     Table interface, returning the controls meeting the criteria in the provided process (by ID). If that's null, starts
        ///     form the root element
        /// </summary>
        public Collection<object> DoTable()
        {
            var result = new Collection<object>();
            foreach (var row in CreateBaseResult())
            {
                var resultRow = new Collection<string>();
                foreach (var cell in row)
                {
                    resultRow.Add(Report(cell));
                }
                result.Add(resultRow);
            }
            return result;
        }

        /// <summary>
        ///     Query interface returning the controls meeting the criteria in the provided process (by ID). If that's null, starts form
        ///     the root element
        /// </summary>
        public Collection<object> Query()
        {
            var table = new Collection<object>();
            var baseResult = CreateBaseResult();

            if (baseResult.Count <= 1) return null;
            var headerRow = baseResult[0];

            foreach (var entry in baseResult.Skip(1))
            {
                var row = new Collection<object>();
                for (var i = 0; i < headerRow.Count; i++)
                {
                    row.Add(new Collection<string> {headerRow[i], entry[i]});
                }
                table.Add(row);
            }
            return table;
        }

        private static string Report(object message) => "report:" + message;
    }
}
