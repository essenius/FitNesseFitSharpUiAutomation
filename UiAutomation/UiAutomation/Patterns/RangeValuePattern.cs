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
using interop.UIAutomationCore;

namespace UiAutomation.Patterns
{
    internal class RangeValuePattern : IPattern
    {
        private readonly IUIAutomationRangeValuePattern _rangeValuePattern;

        public RangeValuePattern(IUIAutomationElement element) => _rangeValuePattern =
            element.GetCurrentPattern(UIA_PatternIds.UIA_RangeValuePatternId) as IUIAutomationRangeValuePattern;

        public bool TryGet(out string returnValue)
        {
            returnValue = string.Empty;
            if (!DoesApply()) return false;
            returnValue = _rangeValuePattern.CurrentValue.ToString(CultureInfo.CurrentCulture);
            return true;
        }

        public SetResult TrySet(string value)
        {
            if (!DoesApply()) return SetResult.NotApplicable;
            try
            {
                _rangeValuePattern.SetValue(Convert.ToDouble(value, CultureInfo.CurrentCulture));
                return SetResult.Success;
            }
            catch (InvalidOperationException)
            {
                // e.g. ScrollBars don't always like the RangeValue pattern, but do seem to handle the legacy pattern fine.
                // Act as if nothing happened, so other options can be attempted.
            }

            return SetResult.NotApplicable;
        }

        private bool DoesApply() => _rangeValuePattern != null;
    }
}
