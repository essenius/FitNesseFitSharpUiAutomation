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

using interop.UIAutomationCore;
using UiAutomation.Model;

namespace UiAutomation.Patterns
{
    internal class SelectionPattern : IPattern
    {
        private const string Delimiter = ";";

        private readonly IUIAutomation _automation;
        private readonly IUIAutomationElement _element;
        private readonly IUIAutomationSelectionPattern _selectionPattern;

        public SelectionPattern(IUIAutomation automation, IUIAutomationElement element)
        {
            _element = element;
            _automation = automation;
            _selectionPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionPatternId) as IUIAutomationSelectionPattern;
        }

        public bool TryGet(out string returnValue)
        {
            returnValue = string.Empty;
            if (!DoesApply()) return false;
            var elementArray = _selectionPattern.GetCurrentSelection();
            for (var i = 0; i < elementArray.Length; i++)
            {
                var item = elementArray.GetElement(i);
                if (i > 0) returnValue += Delimiter;
                returnValue += item.CurrentName;
            }

            return true;
        }

        public SetResult TrySet(string value)
        {
            if (!DoesApply()) return SetResult.NotApplicable;
            _element.ExpandAll(_automation, 0);

            if (string.IsNullOrWhiteSpace(value))
            {
                DeselectAll(_selectionPattern.GetCurrentSelection());
                return SetResult.Success;
            }

            var condition = _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_NamePropertyId, value);
            var item = _element.FindFirst(TreeScope.TreeScope_Descendants, condition);
            return item == null
                ? SetResult.Failure
                : new SelectionItemPattern(item, _selectionPattern.CurrentCanSelectMultiple != 0).TrySet(SelectionItemPattern
                    .SelectValue);
        }

        private static void DeselectAll(IUIAutomationElementArray elementArray)
        {
            for (var i = 0; i < elementArray.Length; i++)
            {
                var selectedItem = elementArray.GetElement(i);
                if (selectedItem.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) is
                    IUIAutomationSelectionItemPattern selectedItemPattern)
                {
                    selectedItemPattern.RemoveFromSelection();
                }
            }
        }

        private bool DoesApply() => _selectionPattern != null;
    }
}