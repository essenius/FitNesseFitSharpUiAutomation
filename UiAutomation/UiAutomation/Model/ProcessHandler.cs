// Copyright 2019 Rik Essenius
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
using System.Diagnostics;
using System.Globalization;
using interop.UIAutomationCore;

namespace UiAutomation.Model
{
    internal class ProcessHandler
    {
        private readonly int? _processId;
        private readonly string _processName;

        public ProcessHandler(int processId) => _processId = processId;

        public ProcessHandler(string searchCriterion)
        {
            var searcher = new SearchParser(searchCriterion);
            if (!searcher.IsValidProcessCondition()) throw new ArgumentException("Could not understand process condition " + searchCriterion);
            var locator = searcher.Locators[0];
            if (locator.ConditionType == UIA_PropertyIds.UIA_ProcessIdPropertyId)
            {
                _processId = Convert.ToInt32(locator.ConditionValue, CultureInfo.InvariantCulture);
            }
            else
            {
                _processName = locator.ConditionValue.ToString();
            }
        }

        public int? Id() => ProcessObject()?.Id;

        public Process ProcessObject()
        {
            if (_processId == null)
            {
                var processes = Process.GetProcessesByName(_processName);
                if (processes.Length == 0 || processes[0].HasExited) return null;
                return processes[0];
            }

            try
            {
                var process = Process.GetProcessById(_processId.Value);
                return process.HasExited ? null : process;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}