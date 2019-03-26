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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using interop.UIAutomationCore;
using UiAutomation.Patterns;

// to make the constants work, set the reference property "Embed Interop Types" for interop.UIAutomationCore to False

namespace UiAutomation.Model
{
    internal class Control
    {
        private const string None = "none";
        private const string Null = "null";
        private static readonly IUIAutomation Automation = new CUIAutomation();

        internal static readonly PropertyMapper Properties = new PropertyMapper();

        private readonly IUIAutomationElement _parent;
        private readonly SearchParser _searchParser;
        private readonly TreeScope _treeScope;

        public Control(Control parent, SearchType searchType, string searchCriterion) :
            this(parent, null, searchType, new SearchParser(searchCriterion))
        {
        }

        private Control(IUIAutomationElement parent, IUIAutomationElement child, SearchType searchType, SearchParser searchParser)
        {
            _parent = parent;
            AutomationElement = child;
            _treeScope = searchType == SearchType.Deep ? TreeScope.TreeScope_Descendants : TreeScope.TreeScope_Children;
            _searchParser = searchParser;
        }

        private Control(Control parent, IUIAutomationElement child, SearchType searchType, SearchParser searchParser) :
            this(parent == null ? Automation.GetRootElement() : parent.AutomationElement, child, searchType, searchParser)
        {
        }

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

        public string Name => AutomationElement == null ? string.Empty : AutomationElement.CurrentName;

        public IUIAutomationElement Parent
        {
            get
            {
                var walker = Automation.ControlViewWalker;
                return walker?.GetParentElement(AutomationElement);
            }
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
            if (!locators.Any() || locators.Count == 1 && string.IsNullOrEmpty(locators[0].Method) && string.IsNullOrEmpty(locators[0].Criterion))
                return Automation.CreateTrueCondition();
            var propertyCondition = locators.Select(locator =>
                Automation.CreatePropertyConditionEx(locator.ConditionType, locator.ConditionValue,
                    PropertyConditionFlags.PropertyConditionFlags_IgnoreCase)).ToList();
            var arr = propertyCondition.ToArray();
            return Automation.CreateAndConditionFromArray(arr);
        }

        internal static Control CreateContainedWindowControl(string containerCriterion, string elementCriterion)
        {
            // Search through all containers satisfying the container criterion for an element satisfying the element criterion.
            // Return null if not found
            var rootElement = Automation.GetRootElement();
            var containerCondition = CreateCondition(new SearchParser(containerCriterion).Locators);

            var containerElements = rootElement.FindAll(TreeScope.TreeScope_Children, containerCondition);
            for (var i = 0; i < containerElements.Length; i++)
            {
                var container = containerElements.GetElement(i);
                var searchParser = new SearchParser(elementCriterion);
                var elementCondition = CreateCondition(searchParser.Locators);
                var element = container.FindFirst(TreeScope.TreeScope_Children, elementCondition);
                // If no such element, try the next container, if any.
                if (element == null) continue;
                return new Control(container, element, SearchType.Shallow, searchParser);
            }
            return null;
        }

        public Control[] DescendantControls(string childSearchCriterion) =>
            FindControls(childSearchCriterion, TreeScope.TreeScope_Descendants, AutomationElement);

        public bool Exists() => AutomationElement != null || FindControl();

        public bool Expand() => AutomationElement != null && AutomationElement.ExpandAll(Automation, 0);

        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "False positive")]
        private static IEnumerable<IUIAutomationElement> FindAllElements(string searchCriterion, TreeScope myTreeScope,
            IUIAutomationElement parent)
        {
            var childSearchParser = new SearchParser(searchCriterion);
            var condition = CreateCondition(childSearchParser.Locators);
            var foundControls = parent?.FindAll(myTreeScope, condition);
            var returnValue = new Collection<IUIAutomationElement>();
            for (var i = 0; i < (foundControls?.Length ?? 0); i++)
            {
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
        /// <param name="searchCriterion">the criterion (method1:locator1 && method2:locator2)</param>
        /// <param name="window">the parent window (in case the control is the window itself)</param>
        /// <returns>the found control</returns>
        public static Control FindControl(string searchCriterion, Control window)
        {
            var searcher = new SearchParser(searchCriterion);
            Control control;
            if (searcher.IsWindowSearch())
            {
                control = window;
            }
            else
            {
                var parent = window == null ? Automation.GetRootElement() : window.AutomationElement;
                control = new Control(parent, null, SearchType.Deep, searcher);
                control.FindControl();
            }

            return control;
        }

        public bool FindControl()
        {
            var condition = CreateCondition(_searchParser.Locators);
            AutomationElement = _parent.FindFirst(_treeScope, condition);
            return AutomationElement != null;
        }

        private static Control[] FindControls(string childSearchCriterion, TreeScope treeScope, IUIAutomationElement parent)
        {
            var elementList = FindAllElements(childSearchCriterion, treeScope, parent);
            return elementList.Select(element => new Control(Automation.GetRootElement(), element, SearchType.Shallow, null)).ToArray();
        }

        private static IUIAutomationElement FindFirstControlUnder(IUIAutomationElement element, IUIAutomationTreeWalker walker, int controlType)
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
            return new FallBackPattern(Automation, targetControl).TryGet(out returnValue) ? returnValue : None;
        }

        public bool IsEnabled() => AutomationElement != null && AutomationElement.CurrentIsEnabled != 0;

        public bool IsVisible() => AutomationElement != null && AutomationElement.CurrentIsOffscreen == 0;

        public object Property(string propertyName)
        {
            var propertyId = int.TryParse(propertyName, out var property) ? property : Properties.Map(propertyName);
            return AutomationElement.GetCurrentPropertyValue(propertyId);
        }

        public static Control[] RootChildControls(string childSearchCriterion) =>
            FindControls(childSearchCriterion, TreeScope.TreeScope_Children, Automation.GetRootElement());

        public string RowNumberContaining(string value)
        {
            if (!(AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_GridPatternId) is IUIAutomationGridPattern
                gridPattern))
            {
                return None;
            }

            for (var x = 0; x < gridPattern.CurrentRowCount; x++)
            for (var y = 0; y < gridPattern.CurrentColumnCount; y++)
            {
                var cell = gridPattern.GetItem(x, y);
                if (GetValue(cell) == value)
                {
                    return (x + 1).ToString(CultureInfo.InvariantCulture);
                }
                // row numbers are zero based, we want 1-based.                        
            }

            return None;
        }

        public bool Select()
        {
            if (!(AutomationElement?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) is
                IUIAutomationSelectionItemPattern selectControl))
            {
                return false;
            }

            selectControl.Select();
            return true;
        }

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
            if (!(AutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_TogglePatternId) is
                IUIAutomationTogglePattern togglePattern))
            {
                return false;
            }

            togglePattern.Toggle();
            return true;
        }

        public bool WaitTillFound() => this.WaitWithTimeoutTill(x => x.FindControl());

        public bool WaitTillNotFound() => this.WaitWithTimeoutTill(x => !x.FindControl());
    }
}