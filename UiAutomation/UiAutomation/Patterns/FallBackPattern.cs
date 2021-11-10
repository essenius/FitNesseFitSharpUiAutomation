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

using System.Threading;
using System.Windows.Forms;
using interop.UIAutomationCore;
using UiAutomation.Model;

namespace UiAutomation.Patterns
{
    internal class FallBackPattern : IPattern
    {
        private readonly IUIAutomation _automation;
        private readonly IUIAutomationElement _element;

        public FallBackPattern(IUIAutomation automation, IUIAutomationElement element)
        {
            _automation = automation;
            _element = element;
        }

        // First see if there is a (potentially non-control-element) child text element and return its name if so. 
        // Otherwise, take the name of the element itself. This is because UWP applications often have these text children to hold the real control value,
        // and the name of the item is an aggregation. Calculator is a good example of that.
        // Also, UWP applications are more likely to include special Unicode characters in the text (e.g. for text direction or alignment); we strip those out.
        public bool TryGet(out string returnValue)
        {
            returnValue =
                FirstControlUnder(_element, _automation.RawViewWalker, UIA_ControlTypeIds.UIA_TextControlTypeId)
                    ?.CurrentName;
            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = _element.CurrentName;
            }
            returnValue = returnValue?.StripUnicodeCharacters();
            return !string.IsNullOrEmpty(returnValue);
        }

        public SetResult TrySet(string value)
        {
            if (!DoesApply()) return SetResult.NotApplicable;
            _element.SetFocus();

            // Pause before sending keyboard input. SendKeys is pretty picky about this
            Thread.Sleep(100);

            // Delete existing content in the control and insert new content.
            SendKeys.SendWait("^a{DEL}");
            Thread.Sleep(500);
            SendKeys.SendWait(value);
            Thread.Sleep(500); // workaround for timing issue with SendKeys
            return SetResult.Success;
        }

        private bool DoesApply() => _element.CurrentIsKeyboardFocusable != 0;

        private static IUIAutomationElement FirstControlUnder(
            IUIAutomationElement element,
            IUIAutomationTreeWalker walker,
            int controlType)
        {
            var child = walker.GetFirstChildElement(element);
            while (child != null)
            {
                if (child.CurrentControlType == controlType) return child;
                var textElement = FirstControlUnder(child, walker, controlType);
                if (textElement != null) return textElement;
                child = walker.GetNextSiblingElement(child);
            }
            return null;
        }
    }
}
