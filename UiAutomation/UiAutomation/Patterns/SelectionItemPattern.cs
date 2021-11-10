// Copyright 2013-2021 Rik Essenius
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
using interop.UIAutomationCore;

namespace UiAutomation.Patterns
{
    internal class SelectionItemPattern : IPattern
    {
        private const string SelectedReturnValue = "Selected";
        private readonly bool _canSelectMultiple;
        private readonly IUIAutomationSelectionItemPattern _selectionItemPattern;

        public SelectionItemPattern(IUIAutomationElement element, bool canSelectMultiple = false)
        {
            _canSelectMultiple = canSelectMultiple;
            _selectionItemPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) as
                IUIAutomationSelectionItemPattern;
        }

        public static string SelectValue => "select";

        public bool TryGet(out string returnValue)
        {
            returnValue = string.Empty;
            if (!DoesApply()) return false;
            returnValue = _selectionItemPattern.CurrentIsSelected == 0 ? string.Empty : SelectedReturnValue;
            return true;
        }

        public SetResult TrySet(string value)
        {
            if (!DoesApply()) return SetResult.NotApplicable;
            if (value.Equals(SelectValue, StringComparison.OrdinalIgnoreCase))
            {
                if (_canSelectMultiple)
                {
                    _selectionItemPattern.AddToSelection();
                }
                else
                {
                    _selectionItemPattern.Select();
                }
                return SetResult.Success;
            }

            if (!string.IsNullOrEmpty(value)) return SetResult.Failure;
            _selectionItemPattern.RemoveFromSelection();
            return SetResult.Success;
        }

        private bool DoesApply() => _selectionItemPattern != null;
    }
}
