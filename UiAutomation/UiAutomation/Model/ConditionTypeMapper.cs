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

namespace UiAutomation.Model;

internal class ConditionTypeMapper : Mapping<int>
{
    public ConditionTypeMapper() : base("Condition Type")
    {
        Add("AccessKey", UIA_PropertyIds.UIA_AccessKeyPropertyId);
        Add("Caption", UIA_PropertyIds.UIA_NamePropertyId);
        Add("ClassName", UIA_PropertyIds.UIA_ClassNamePropertyId);
        Add("ControlType", UIA_PropertyIds.UIA_ControlTypePropertyId);
        Add("HelpText", UIA_PropertyIds.UIA_HelpTextPropertyId);
        Add("Id", UIA_PropertyIds.UIA_AutomationIdPropertyId);
        Add("IsEnabled", UIA_PropertyIds.UIA_IsEnabledPropertyId);
        Add("IsPassword", UIA_PropertyIds.UIA_IsPasswordPropertyId);
        Add("Name", UIA_PropertyIds.UIA_NamePropertyId);
        Add("ProcessId", UIA_PropertyIds.UIA_ProcessIdPropertyId);
        Add("WindowHandle", UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
    }

    public static bool IsBooleanType(string conditionType) =>
        conditionType.StartsWith("Is", StringComparison.Ordinal);

    public static bool IsControlType(string conditionType) =>
        conditionType.Equals("ControlType", StringComparison.OrdinalIgnoreCase);

    public static bool IsNumericalType(string conditionType) =>
        conditionType.Equals("ProcessId", StringComparison.OrdinalIgnoreCase) ||
        conditionType.Equals("WindowHandle", StringComparison.OrdinalIgnoreCase);
}