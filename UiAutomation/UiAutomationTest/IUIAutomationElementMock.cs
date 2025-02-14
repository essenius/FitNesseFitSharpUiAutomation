// Copyright 2024 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using interop.UIAutomationCore;

// ReSharper disable UnassignedGetOnlyAutoProperty -- must define to match interface

namespace UiAutomationTest;

// ReSharper disable once InconsistentNaming -- matching Microsoft convention
public class IUIAutomationElementMock : IUIAutomationElement
{
    public void SetFocus()
    {
    }

    public int[] GetRuntimeId() => throw new NotImplementedException();

    public IUIAutomationElement FindFirst(TreeScope scope, IUIAutomationCondition condition) => throw new NotImplementedException();

    public IUIAutomationElementArray FindAll(TreeScope scope, IUIAutomationCondition condition) => throw new NotImplementedException();

    public IUIAutomationElement FindFirstBuildCache(TreeScope scope, IUIAutomationCondition condition, IUIAutomationCacheRequest cacheRequest) =>
        throw new NotImplementedException();

    public IUIAutomationElementArray FindAllBuildCache(TreeScope scope, IUIAutomationCondition condition, IUIAutomationCacheRequest cacheRequest) =>
        throw new NotImplementedException();

    public IUIAutomationElement BuildUpdatedCache(IUIAutomationCacheRequest cacheRequest) => throw new NotImplementedException();

    public object GetCurrentPropertyValue(int propertyId) => throw new NotImplementedException();

    public object GetCurrentPropertyValueEx(int propertyId, int ignoreDefaultValue) => throw new NotImplementedException();

    public object GetCachedPropertyValue(int propertyId) => throw new NotImplementedException();

    public object GetCachedPropertyValueEx(int propertyId, int ignoreDefaultValue) => throw new NotImplementedException();

    public IntPtr GetCurrentPatternAs(int patternId, ref Guid riid) => throw new NotImplementedException();

    public IntPtr GetCachedPatternAs(int patternId, ref Guid riid) => throw new NotImplementedException();

    public object GetCurrentPattern(int patternId) => null;

    public object GetCachedPattern(int patternId) => throw new NotImplementedException();

    public IUIAutomationElement GetCachedParent() => throw new NotImplementedException();

    public IUIAutomationElementArray GetCachedChildren() => throw new NotImplementedException();

    public int GetClickablePoint([UnscopedRef] out tagPOINT clickable) => throw new NotImplementedException();

    public int CurrentProcessId { get; }
    public int CurrentControlType { get; }
    public string CurrentLocalizedControlType { get; }
    public string CurrentName { get; set; }
    public string CurrentAcceleratorKey { get; }
    public string CurrentAccessKey { get; }
    public int CurrentHasKeyboardFocus { get; }
    public int CurrentIsKeyboardFocusable { get; set; }
    public int CurrentIsEnabled { get; }
    public string CurrentAutomationId { get; }
    public string CurrentClassName { get; }
    public string CurrentHelpText { get; }
    public int CurrentCulture { get; }
    public int CurrentIsControlElement { get; }
    public int CurrentIsContentElement { get; }
    public int CurrentIsPassword { get; }
    public IntPtr CurrentNativeWindowHandle { get; }
    public string CurrentItemType { get; }
    public int CurrentIsOffscreen { get; }
    public OrientationType CurrentOrientation { get; }
    public string CurrentFrameworkId { get; }
    public int CurrentIsRequiredForForm { get; }
    public string CurrentItemStatus { get; }
    public tagRECT CurrentBoundingRectangle { get; }
    public IUIAutomationElement CurrentLabeledBy { get; }
    public string CurrentAriaRole { get; }
    public string CurrentAriaProperties { get; }
    public int CurrentIsDataValidForForm { get; }
    public IUIAutomationElementArray CurrentControllerFor { get; }
    public IUIAutomationElementArray CurrentDescribedBy { get; }
    public IUIAutomationElementArray CurrentFlowsTo { get; }
    public string CurrentProviderDescription { get; }
    public int CachedProcessId { get; }
    public int CachedControlType { get; }
    public string CachedLocalizedControlType { get; }
    public string CachedName { get; }
    public string CachedAcceleratorKey { get; }
    public string CachedAccessKey { get; }
    public int CachedHasKeyboardFocus { get; }
    public int CachedIsKeyboardFocusable { get; }
    public int CachedIsEnabled { get; }
    public string CachedAutomationId { get; }
    public string CachedClassName { get; }
    public string CachedHelpText { get; }
    public int CachedCulture { get; }
    public int CachedIsControlElement { get; }
    public int CachedIsContentElement { get; }
    public int CachedIsPassword { get; }
    public IntPtr CachedNativeWindowHandle { get; }
    public string CachedItemType { get; }
    public int CachedIsOffscreen { get; }
    public OrientationType CachedOrientation { get; }
    public string CachedFrameworkId { get; }
    public int CachedIsRequiredForForm { get; }
    public string CachedItemStatus { get; }
    public tagRECT CachedBoundingRectangle { get; }
    public IUIAutomationElement CachedLabeledBy { get; }
    public string CachedAriaRole { get; }
    public string CachedAriaProperties { get; }
    public int CachedIsDataValidForForm { get; }
    public IUIAutomationElementArray CachedControllerFor { get; }
    public IUIAutomationElementArray CachedDescribedBy { get; }
    public IUIAutomationElementArray CachedFlowsTo { get; }
    public string CachedProviderDescription { get; }
}
