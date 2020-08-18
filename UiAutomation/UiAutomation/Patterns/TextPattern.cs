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
using interop.UIAutomationCore;

namespace UiAutomation.Patterns
{
    internal class TextPattern : IPattern
    {
        private readonly IUIAutomationTextPattern _textPattern;

        public TextPattern(IUIAutomationElement element) =>
            _textPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_TextPatternId) as IUIAutomationTextPattern;

        public bool TryGet(out string returnValue)
        {
            returnValue = string.Empty;
            if (!DoesApply()) return false;
            returnValue = _textPattern.DocumentRange.GetText(-1).Trim();
            return true;
        }

        public SetResult TrySet(string value) => throw new NotImplementedException();

        private bool DoesApply() => _textPattern != null;
    }
}
