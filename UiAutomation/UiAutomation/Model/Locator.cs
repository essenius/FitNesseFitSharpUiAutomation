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
            if (locatorString.Contains(TypeDelimiter))
            {
                Method = locatorString.Substring(0, locatorString.IndexOf(TypeDelimiter, StringComparison.Ordinal)).Trim();
                Criterion = locatorString.Substring(locatorString.IndexOf(TypeDelimiter, StringComparison.Ordinal) + 1).Trim();
            }
            else
            {
                Method = DefaultConditionType;
                Criterion = locatorString.Trim();
            }
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

        public bool IsWindowSearch => ConditionTypeMapper.IsControlType(Method) && ControlTypeMapper.IsWindow(UnescapedCriterion);

        public string Method { get; }

        private string UnescapedCriterion => Regex.Unescape(Criterion);
    }
}