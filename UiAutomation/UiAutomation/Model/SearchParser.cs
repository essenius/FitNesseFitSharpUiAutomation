// Copyright 2013-2020 Rik Essenius
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
using System.Linq;
using System.Text.RegularExpressions;
using interop.UIAutomationCore;

// based on SearchParser in the SeleniumFixture 

namespace UiAutomation.Model
{
    internal class LocatorCollection : List<Locator>
    {
    }

    internal class SearchParser
    {
        private const string AndDelimiter = " && ";

        public SearchParser(string searchCriteriaString)
        {
            if (searchCriteriaString == null) // empty string is allowed, null isn't
            {
                throw new ArgumentNullException(nameof(searchCriteriaString), "SearchParser requires non-null search criteria");
            }

            var searchCriteria = Regex.Split(searchCriteriaString, AndDelimiter);
            Locators = new LocatorCollection();
            foreach (var locator in searchCriteria)
            {
                Locators.Add(new Locator(locator));
            }
        }

        public LocatorCollection Locators { get; }

        public bool IsValidProcessCondition()
        {
            var validProcessConditions = new List<int> {UIA_PropertyIds.UIA_ProcessIdPropertyId, UIA_PropertyIds.UIA_NamePropertyId};
            return Locators.Count == 1 && validProcessConditions.Contains(Locators[0].ConditionType);
        }

        public bool IsWindowSearch() => Locators.Any(locator => locator.IsWindowSearch);
    }
}
