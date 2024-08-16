// Copyright 2013-2024 Rik Essenius
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

namespace UiAutomation.Patterns;

internal class TogglePattern(IUIAutomationElement element) : IPattern
{
    private readonly IUIAutomationTogglePattern _togglePattern = element.GetCurrentPattern(UIA_PatternIds.UIA_TogglePatternId) as IUIAutomationTogglePattern;

    public bool TryGet(out string returnValue)
    {
        returnValue = string.Empty;
        if (!DoesApply()) return false;
        var stringResult = _togglePattern.CurrentToggleState.ToString();
        returnValue = stringResult[(stringResult.IndexOf("_", StringComparison.Ordinal) + 1)..];
        return true;
    }

    public SetResult TrySet(string value) => SetResult.NotApplicable;

    private bool DoesApply() => _togglePattern != null;
}
