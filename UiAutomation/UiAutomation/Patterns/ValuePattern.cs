﻿// Copyright 2013-2024 Rik Essenius
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

internal class ValuePattern(IUIAutomationElement element) : IPattern
{
    private readonly IUIAutomationValuePattern _valuePattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;

    public bool TryGet(out string returnValue)
    {
        returnValue = string.Empty;
        if (!DoesApply()) return false;
        try
        {
            returnValue = _valuePattern.CurrentValue.Trim();
        }
        catch (InvalidOperationException)
        {
            // this happens e.g. with password boxes
            return false;
        }

        return true;
    }

    public SetResult TrySet(string value)
    {
        if (!DoesApply()) return SetResult.NotApplicable;
        if (_valuePattern.CurrentIsReadOnly != 0) return SetResult.Failure;
        // We assume that this always succeeds. Since we already checked whether this is enabled and read/write, 
        // this is a fair assumption, except for the title bar (which now succeeds despite not changing anything).
        _valuePattern.SetValue(value);
        return SetResult.Success;
    }

    private bool DoesApply() => _valuePattern != null;
}