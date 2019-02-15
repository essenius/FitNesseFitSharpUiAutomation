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
using System.Diagnostics;
using interop.UIAutomationCore;

namespace UiAutomation.Patterns
{
    internal class LegacyIAccessiblePattern : IPattern
    {
        private readonly IUIAutomationLegacyIAccessiblePattern _legacyIAccessiblePattern;

        public LegacyIAccessiblePattern(IUIAutomationElement element) => _legacyIAccessiblePattern = element.GetCurrentPattern(
            UIA_PatternIds.UIA_LegacyIAccessiblePatternId) as IUIAutomationLegacyIAccessiblePattern;

        public bool TryGet(out string returnValue) => throw new NotImplementedException();

        public SetResult TrySet(string value)
        {
            if (!DoesApply()) return SetResult.NotApplicable;
            try
            {
                _legacyIAccessiblePattern.SetValue(value);
                // legacy IAccessible pattern sometimes claims to succeed without doing anything, so check for that.
                if (_legacyIAccessiblePattern.CurrentValue == value) return SetResult.Success;
            }
            catch (NotImplementedException)
            {
                // ignore, report that it didn't succeed
            }

            return SetResult.NotApplicable;
        }

        private bool DoesApply() => _legacyIAccessiblePattern != null;
    }
}