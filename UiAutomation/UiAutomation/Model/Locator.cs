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
using System.Globalization;
using System.Text.RegularExpressions;

namespace UiAutomation.Model
{
    internal class Locator
    {
        private const string TypeDelimiter = ":";
        private static string _defaultMethod = "Name";

        private static readonly ConditionTypeMapper ConditionTypes = new ConditionTypeMapper();
        private static readonly ControlTypeMapper ControlTypes = new ControlTypeMapper();

        public Locator(string locatorString)
        {
            string criterion;
            if (locatorString.Contains(TypeDelimiter))
            {
                Method = locatorString.Substring(0, locatorString.IndexOf(TypeDelimiter, StringComparison.Ordinal)).Trim();
                criterion = locatorString.Substring(locatorString.IndexOf(TypeDelimiter, StringComparison.Ordinal) + 1).Trim();
            }
            else
            {
                Method = DefaultConditionType;
                criterion = locatorString.Trim();
            }
            // Find non-whitespace, then optional whitespace, and then text between brackets. If it matches, we have a grid item specification
            var match = Regex.Match(criterion, @"([^\s]*)\s*\[(.*)\]", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                Criterion = criterion;
                GridItem = string.Empty;
                return;
            }
            Criterion = match.Groups[1].Value;
            GridItem = match.Groups[2].Value;
        }

        public int ConditionType => ConditionTypes.Map(Method);

        public object ConditionValue
        {
            get
            {
                if (ConditionTypeMapper.IsControlType(Method)) return ControlType;
                if (ConditionTypeMapper.IsNumericalType(Method)) return Convert.ToInt32(Criterion, CultureInfo.CurrentCulture);
                if (ConditionTypeMapper.IsBooleanType(Method)) return bool.Parse(Criterion);
                return UnescapedCriterion;
            }
        }

        private int ControlType => ControlTypes.Map(Criterion);
        public string Criterion { get; }

        public static string DefaultConditionType
        {
            get => _defaultMethod;
            set
            {
                if (ConditionTypes.ContainsKey(value)) _defaultMethod = value;
            }
        }

        public string GridItem { get; }

        public bool IsWindowSearch => ConditionTypeMapper.IsControlType(Method) && ControlTypeMapper.IsWindow(UnescapedCriterion);

        public string Method { get; }

        private string UnescapedCriterion => Regex.Unescape(Criterion);
    }
}
