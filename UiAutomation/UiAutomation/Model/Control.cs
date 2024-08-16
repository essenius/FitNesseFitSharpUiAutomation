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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using interop.UIAutomationCore;
using UiAutomation.Patterns;
using PropertyConditionFlags = interop.UIAutomationCore.PropertyConditionFlags;
using RangeValuePattern = UiAutomation.Patterns.RangeValuePattern;
using SelectionItemPattern = UiAutomation.Patterns.SelectionItemPattern;
using SelectionPattern = UiAutomation.Patterns.SelectionPattern;
using TextPattern = UiAutomation.Patterns.TextPattern;
using TogglePattern = UiAutomation.Patterns.TogglePattern;
using TreeScope = interop.UIAutomationCore.TreeScope;
using ValuePattern = UiAutomation.Patterns.ValuePattern;

// to make the constants work, set the reference property "Embed Interop Types" for interop.UIAutomationCore to False

namespace UiAutomation.Model;

internal class Control
{
    private const string Null = "null";
    private static readonly IUIAutomation Automation = new CUIAutomation();
    private static readonly PropertyMapper Properties = [];
    private IUIAutomationElement _parent;
    private string _searchCriterion;
    private SearchParser _searchParser;
    private TreeScope _treeScope = TreeScope.TreeScope_Descendants;

    public Control(string searchCriterion) => SearchCriterion = searchCriterion;

    public IUIAutomationElement AutomationElement { get; private set; }

    public string AutomationId => AutomationElement == null ? string.Empty : AutomationElement.CurrentAutomationId;

    public int ColumnCount
    {
        get
        {
            if (AutomationElement == null) return 0;
            var gridPattern = AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId) as IUIAutomationGridPattern;
            return gridPattern?.CurrentColumnCount ?? 0;
        }
    }

    public IUIAutomationElement FindParentElement
    {
        get
        {
            var walker = Automation.ControlViewWalker;
            return walker.GetParentElement(AutomationElement);
        }
    }

    public string Name => AutomationElement == null ? string.Empty : AutomationElement.CurrentName;

    public Control Parent
    {
        set => _parent = value == null ? Automation.GetRootElement() : value.AutomationElement;
    }

    public int RowCount
    {
        get
        {
            if (AutomationElement == null) return 0;
            var gridPattern = AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId) as IUIAutomationGridPattern;
            return gridPattern?.CurrentRowCount ?? 0;
        }
    }

    public string SearchCriterion
    {
        get => _searchCriterion;
        private set
        {
            _searchCriterion = value;
            _searchParser = _searchCriterion == null ? null : new SearchParser(value);
        }
    }

    public SearchType SearchType
    {
        get => _treeScope == TreeScope.TreeScope_Descendants ? SearchType.Deep : SearchType.Shallow;
        set => _treeScope = value == SearchType.Deep
            ? TreeScope.TreeScope_Descendants
            : TreeScope.TreeScope_Children;
    }

    public string Value => GetValue(AutomationElement);

    public IntPtr WindowHandle
    {
        // if the handle is 0, then it's not a visible window and we need to go up in the tree
        get
        {
            var currentElement = AutomationElement;
            while (currentElement != null && currentElement.CurrentNativeWindowHandle == IntPtr.Zero)
            {
                currentElement = currentElement.FindFirst(TreeScope.TreeScope_Parent, Automation.CreateTrueCondition());
            }

            return currentElement?.CurrentNativeWindowHandle ?? IntPtr.Zero;
        }
    }

    public GridItem CellContaining(string value)
    {
        if (AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId) is not IUIAutomationGridPattern gridPattern)
        {
            return null;
        }

        for (var row = 0; row < gridPattern.CurrentRowCount; row++)
            for (var column = 0; column < gridPattern.CurrentColumnCount; column++)
            {
                var cell = gridPattern.GetItem(row, column);
                if (GetValue(cell) == value)
                {
                    return new GridItem(row + 1, column + 1);
                }
                // row numbers are zero based, we want 1-based.  
            }

        return null;
    }

    public bool Click()
    {
        if (AutomationElement == null) return false;
        if (AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) is IUIAutomationInvokePattern invokePattern)
        {
            try
            {
                invokePattern.Invoke();
                return true;
            }
            catch (COMException)
            {
                // some buttons (especially min/max/close) don't behave right on Invoke. Retry via mouse click.
            }
        }

        // if the invoke pattern didn't exist, retry via a simulation of a mouse click.
        return Mouse.Click(AutomationElement);
    }

    public bool Collapse() => AutomationElement != null && AutomationElement.CollapseAll(Automation);

    private static IUIAutomationCondition CreateCondition(LocatorCollection locators)
    {
        if (!locators.Any() || (locators.Count == 1 && string.IsNullOrEmpty(locators[0].Method) && string.IsNullOrEmpty(locators[0].Criterion)))
        {
            return Automation.CreateTrueCondition();
        }

        var propertyCondition = locators.Select(locator =>
            Automation.CreatePropertyConditionEx(locator.ConditionType, locator.ConditionValue, PropertyConditionFlags.PropertyConditionFlags_IgnoreCase)
        ).ToList();
        var arr = propertyCondition.ToArray();
        return Automation.CreateAndConditionFromArray(arr);
    }

    /// <summary>
    ///     Search all windows directly under the root element for an window with a specific process ID.
    ///     Can also be the window itself.
    /// </summary>
    /// <param name="processId">the process ID to search for</param>
    /// <returns>the found window control</returns>
    internal static Control CreateContainedWindowControl(int processId)
    {
        const string windowLocator = "ControlType:Window";
        var rootElement = Automation.GetRootElement();
        var containerCondition = CreateCondition(new SearchParser(windowLocator).Locators);
        var containerElements = rootElement.FindAll(TreeScope.TreeScope_Descendants, containerCondition);
        var elementCriterion = $"ProcessId:{processId} && {windowLocator}";
        for (var i = 0; i < containerElements.Length; i++)
        {
            var container = containerElements.GetElement(i);
            IUIAutomationElement element;
            IUIAutomationElement parent;
            if (container.CurrentProcessId == processId)
            {
                element = container;
                parent = rootElement;
            }
            else
            {
                var searchParser = new SearchParser(elementCriterion);
                var elementCondition = CreateCondition(searchParser.Locators);
                element = container.FindFirst(TreeScope.TreeScope_Children, elementCondition);
                parent = container;
            }

            // If no such element, try the next container, if any.
            if (element == null) continue;
            return new Control(elementCriterion) { SearchType = SearchType.Deep, AutomationElement = element, _parent = parent };
        }

        return null;
    }

    public Control[] DescendantControls(string childSearchCriterion) =>
        FindControls(childSearchCriterion, TreeScope.TreeScope_Descendants, AutomationElement);

    public bool DoubleClick() => AutomationElement != null && Mouse.DoubleClick(AutomationElement);

    public bool Exists() => AutomationElement != null || FindControl();

    public bool Expand() => AutomationElement != null && AutomationElement.ExpandAll(Automation, 0);

    private static IEnumerable<IUIAutomationElement> FindAllElements(
        string searchCriterion,
        TreeScope myTreeScope,
        IUIAutomationElement parent)
    {
        var childSearchParser = new SearchParser(searchCriterion);
        var condition = CreateCondition(childSearchParser.Locators);
        var foundControls = parent?.FindAll(myTreeScope, condition);
        var returnValue = new Collection<IUIAutomationElement>();
        for (var i = 0; i < (foundControls?.Length ?? 0); i++)
        {
            // ReSharper disable once PossibleNullReferenceException -- can't be null because of the loop condition
            returnValue.Add(foundControls.GetElement(i));
        }

        return returnValue;
    }

    public IEnumerable<IUIAutomationElement> FindAllElements(string searchCriterion) =>
        FindAllElements(searchCriterion, _treeScope, AutomationElement);

    // I wanted to create a WaitForInputIdle method based on the WindowPattern implementation, but got a not implemented exception ...

    /// <summary>
    ///     Return a control based on a search criterion
    /// </summary>
    /// <param name="searchCriterion">the criterion (method1:locator1 &amp;&amp; method2:locator2)</param>
    /// <param name="window">the parent window (in case the control is the window itself)</param>
    /// <returns>the found control</returns>
    public static Control FindControl(string searchCriterion, Control window)
    {
        var searcher = new SearchParser(searchCriterion);
        Control control;
        if (searcher.IsMainWindowSearch())
        {
            control = window;
        }
        else
        {
            var parent = window == null ? Automation.GetRootElement() : window.AutomationElement;
            control = new Control(searchCriterion) { SearchType = SearchType.Deep, _parent = parent };
            control.FindControl();
        }

        return control;
    }

    public bool FindControl()
    {
        if (_searchParser.IsMainWindowSearch())
        {
            AutomationElement = _parent;
        }
        else
        {
            var condition = CreateCondition(_searchParser.Locators);
            AutomationElement = _parent.FindFirst(_treeScope, condition);
        }

        if (AutomationElement == null) return false;

        // If we have a grid item locator, find the corresponding item (i.e. replace the parent by AutomationElement,
        // and replace AutomationElement by that of the grid item
        var gridItemLocator = GridItemLocator(_searchParser.Locators);
        return string.IsNullOrEmpty(gridItemLocator) || FindGridItem(new GridItem(gridItemLocator));
    }

    private static Control[] FindControls(
        string childSearchCriterion,
        TreeScope treeScope,
        IUIAutomationElement parent)
    {
        var elementList = FindAllElements(childSearchCriterion, treeScope, parent);
        return elementList.Select(element => new Control(null) { AutomationElement = element, Parent = null, SearchType = SearchType.Shallow }).ToArray();
        //return elementList.Select(element => new Control(Automation.GetRootElement(), element, SearchType.Shallow, null)).ToArray();
    }

    private static IUIAutomationElement FindFirstControlUnder(
        IUIAutomationElement element,
        IUIAutomationTreeWalker walker,
        int controlType)
    {
        var child = walker.GetFirstChildElement(element);
        while (child != null)
        {
            if (child.CurrentControlType == controlType) return child;
            var textElement = FindFirstControlUnder(child, walker, controlType);
            if (textElement != null) return textElement;
            child = walker.GetNextSiblingElement(child);
        }

        return null;
    }

    public bool FindGridItem(GridItem gridItem)
    {
        if (AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId) is not IUIAutomationGridPattern gridPattern) return false;
        if (gridItem.GridItemType == GridItemType.Cell)
        {
            _parent = AutomationElement;
            AutomationElement = gridPattern.GetItem(gridItem.Row - 1, gridItem.Column - 1);
            return AutomationElement != null;
        }

        // we have GridItemTypes Row and Column left. Get the respective row or column header.
        if (AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_TablePatternId) is not IUIAutomationTablePattern tablePattern) return false;
        var isRow = gridItem.GridItemType == GridItemType.Row;
        var headers = isRow ? tablePattern.GetCurrentRowHeaders() : tablePattern.GetCurrentColumnHeaders();
        var index = isRow ? gridItem.Row : gridItem.Column;
        var header = headers.GetElement(index - 1);
        _parent = AutomationElement;
        AutomationElement = header;
        return true;
    }

    private static IUIAutomationElement FirstDataCellIn(IUIAutomationElement dataItemElement)
    {
        if (dataItemElement == null) return null;

        var walker = Automation.ControlViewWalker;
        var child = walker.GetFirstChildElement(dataItemElement);

        while (child != null)
        {
            if (child.CurrentControlType != UIA_ControlTypeIds.UIA_HeaderItemControlTypeId) return child;
            child = walker.GetNextSiblingElement(child);
        }

        return null;
    }

    public string FirstTextUnder()
    {
        if (AutomationElement == null) return null;
        var walker = Automation.RawViewWalker;
        var child = FindFirstControlUnder(AutomationElement, walker, UIA_ControlTypeIds.UIA_TextControlTypeId);
        return child?.CurrentName?.StripUnicodeCharacters();
    }

    internal static string GetValue(IUIAutomationElement targetControl)
    {
        if (targetControl == null) return Null;
        if (new ValuePattern(targetControl).TryGet(out var returnValue)) return returnValue;
        if (new RangeValuePattern(targetControl).TryGet(out returnValue)) return returnValue;
        if (new TextPattern(targetControl).TryGet(out returnValue)) return returnValue;
        if (new TogglePattern(targetControl).TryGet(out returnValue)) return returnValue;
        if (new SelectionPattern(Automation, targetControl).TryGet(out returnValue)) return returnValue;
        if (new SelectionItemPattern(targetControl).TryGet(out returnValue)) return returnValue;
        return new FallBackPattern(Automation, targetControl).TryGet(out returnValue) ? returnValue : null;
    }

    private static string GridItemLocator(LocatorCollection locators)
    {
        foreach (var locator in locators)
        {
            if (!string.IsNullOrEmpty(locator.GridItem)) return locator.GridItem;
        }

        return string.Empty;
    }

    public bool IsEnabled() => AutomationElement != null && AutomationElement.CurrentIsEnabled != 0;

    public bool IsVisible() => AutomationElement is { CurrentIsOffscreen: 0 };

    public static Control Parse(string searchCriterion) => new(searchCriterion) { Parent = null };

    public object Property(string propertyName)
    {
        var propertyId = int.TryParse(propertyName, out var property) ? property : Properties.Map(propertyName);
        return AutomationElement.GetCurrentPropertyValue(propertyId);
    }

    public static Control[] RootChildControls(string childSearchCriterion) =>
        FindControls(childSearchCriterion, TreeScope.TreeScope_Children, Automation.GetRootElement());

    public bool Select()
    {
        if (AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) is not IUIAutomationSelectionItemPattern selectControl)
        {
            return false;
        }

        selectControl.Select();
        return true;
    }

    public GridItem SelectedCell()
    {
        if (AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionPatternId) is not IUIAutomationSelectionPattern pattern) return null;

        var items = pattern.GetCurrentSelection();
        if (items.Length == 0) return null;
        // If multiple items are selected, we just return the first one.
        var selectedItem = items.GetElement(0);

        // If the selected item is a row, find the first cell in the row.
        if (selectedItem.CurrentControlType == UIA_ControlTypeIds.UIA_DataItemControlTypeId)
        {
            selectedItem = FirstDataCellIn(selectedItem);
        }

        return selectedItem?.GetCurrentPattern(UIA_PatternIds.UIA_GridItemPatternId) is not IUIAutomationGridItemPattern gridItemPattern
            ? null
            : new GridItem(gridItemPattern.CurrentRow + 1, gridItemPattern.CurrentColumn + 1);
    }

    public IUIAutomationElementArray SelectedItems() =>
        AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionPatternId) is not IUIAutomationSelectionPattern pattern
            ? null
            : pattern.GetCurrentSelection();

    public bool SetValue(string value)
    {
        if (!IsEnabled()) return false;
        var result = new SelectionPattern(Automation, AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        result = new RangeValuePattern(AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        result = new ValuePattern(AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        result = new SelectionItemPattern(AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        result = new LegacyIAccessiblePattern(AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        result = new FallBackPattern(Automation, AutomationElement).TrySet(value);
        if (result != SetResult.NotApplicable) return result == SetResult.Success;
        return false;
    }

    public bool Toggle()
    {
        if (!IsEnabled() || AutomationElement.CurrentIsKeyboardFocusable == 0) return false;
        AutomationElement.SetFocus();
        if (AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_TogglePatternId) is not IUIAutomationTogglePattern togglePattern)
        {
            return false;
        }

        togglePattern.Toggle();
        return true;
    }

    public bool WaitTillFound() => this.WaitWithTimeoutTill(x => x.FindControl());

    public bool WaitTillNotFound() => this.WaitWithTimeoutTill(x => !x.FindControl());
}
