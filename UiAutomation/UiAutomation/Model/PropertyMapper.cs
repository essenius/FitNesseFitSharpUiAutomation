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

namespace UiAutomation.Model
{
    internal class PropertyMapper : Mapping<int>
    {
        public PropertyMapper() : base("Property")
        {
            Add("AcceleratorKey", UIA_PropertyIds.UIA_AcceleratorKeyPropertyId);
            Add("AccessKey", UIA_PropertyIds.UIA_AccessKeyPropertyId);
            Add("AriaProperties", UIA_PropertyIds.UIA_AriaPropertiesPropertyId);
            Add("AriaRole", UIA_PropertyIds.UIA_AriaRolePropertyId);
            Add("AutomationId", UIA_PropertyIds.UIA_AutomationIdPropertyId);
            Add("BoundingRectangle", UIA_PropertyIds.UIA_BoundingRectanglePropertyId);
            Add("CanMaximize", UIA_PropertyIds.UIA_WindowCanMaximizePropertyId);
            Add("CanMinimize", UIA_PropertyIds.UIA_WindowCanMinimizePropertyId);
            Add("CanMove", UIA_PropertyIds.UIA_TransformCanMovePropertyId);
            Add("CanResize", UIA_PropertyIds.UIA_TransformCanResizePropertyId);
            Add("CanRotate", UIA_PropertyIds.UIA_TransformCanRotatePropertyId);
            Add("CanSelectMultiple", UIA_PropertyIds.UIA_SelectionCanSelectMultiplePropertyId);
            Add("ClassName", UIA_PropertyIds.UIA_ClassNamePropertyId);
            Add("ClickablePoint", UIA_PropertyIds.UIA_ClickablePointPropertyId);
            Add("ControlType", UIA_PropertyIds.UIA_ControlTypePropertyId);
            Add("Culture", UIA_PropertyIds.UIA_CulturePropertyId);
            Add("DockPosition", UIA_PropertyIds.UIA_DockDockPositionPropertyId);
            Add("ExpandCollapseState", UIA_PropertyIds.UIA_ExpandCollapseExpandCollapseStatePropertyId);
            Add("FrameworkId", UIA_PropertyIds.UIA_FrameworkIdPropertyId);
            Add("ColumnCount", UIA_PropertyIds.UIA_GridColumnCountPropertyId);
            Add("Column", UIA_PropertyIds.UIA_GridItemColumnPropertyId);
            Add("ColumnSpan", UIA_PropertyIds.UIA_GridItemColumnSpanPropertyId);
            Add("Row", UIA_PropertyIds.UIA_GridItemRowPropertyId);
            Add("RowSpan", UIA_PropertyIds.UIA_GridItemRowSpanPropertyId);
            Add("RowCount", UIA_PropertyIds.UIA_GridRowCountPropertyId);
            Add("HasKeyboardFocus", UIA_PropertyIds.UIA_HasKeyboardFocusPropertyId);
            Add("HelpText", UIA_PropertyIds.UIA_HelpTextPropertyId);
            Add("HorizontallyScrollable", UIA_PropertyIds.UIA_ScrollHorizontallyScrollablePropertyId);
            Add("HorizontalScrollPercent", UIA_PropertyIds.UIA_ScrollHorizontalScrollPercentPropertyId);
            Add("HorizontalViewSize", UIA_PropertyIds.UIA_ScrollHorizontalViewSizePropertyId);
            Add("InteractionState", UIA_PropertyIds.UIA_WindowWindowInteractionStatePropertyId);
            Add("IsContentElement", UIA_PropertyIds.UIA_IsContentElementPropertyId);
            Add("IsControlElement", UIA_PropertyIds.UIA_IsControlElementPropertyId);
            Add("IsDataValidForForm", UIA_PropertyIds.UIA_IsDataValidForFormPropertyId);
            Add("IsEnabled", UIA_PropertyIds.UIA_IsEnabledPropertyId);
            Add("IsKeyboardFocusable", UIA_PropertyIds.UIA_IsKeyboardFocusablePropertyId);
            Add("IsModal", UIA_PropertyIds.UIA_WindowIsModalPropertyId);
            Add("IsOffscreen", UIA_PropertyIds.UIA_IsOffscreenPropertyId);
            Add("IsPassword", UIA_PropertyIds.UIA_IsPasswordPropertyId);
            Add("IsReadOnly", UIA_PropertyIds.UIA_ValueIsReadOnlyPropertyId);
            Add("IsSelectionRequired", UIA_PropertyIds.UIA_SelectionIsSelectionRequiredPropertyId);
            Add("IsTopmost", UIA_PropertyIds.UIA_WindowIsTopmostPropertyId);
            Add("ItemIsSelected", UIA_PropertyIds.UIA_SelectionItemIsSelectedPropertyId);
            Add("ItemStatus", UIA_PropertyIds.UIA_ItemStatusPropertyId);
            Add("ItemType", UIA_PropertyIds.UIA_ItemTypePropertyId);
            Add("LabeledBy", UIA_PropertyIds.UIA_LabeledByPropertyId);
            Add("LocalizedControlType", UIA_PropertyIds.UIA_LocalizedControlTypePropertyId);
            Add("Name", UIA_PropertyIds.UIA_NamePropertyId);
            Add("NativeWindowHandle", UIA_PropertyIds.UIA_NativeWindowHandlePropertyId);
            Add("Orientation", UIA_PropertyIds.UIA_OrientationPropertyId);
            Add("ProcessId", UIA_PropertyIds.UIA_ProcessIdPropertyId);
            Add("ProviderDescription", UIA_PropertyIds.UIA_ProviderDescriptionPropertyId);
            Add("ToggleState", UIA_PropertyIds.UIA_ToggleToggleStatePropertyId);
            Add("Value", UIA_PropertyIds.UIA_ValueValuePropertyId);
            Add("VerticallyScrollable", UIA_PropertyIds.UIA_ScrollVerticallyScrollablePropertyId);
            Add("VerticalScrollPercent", UIA_PropertyIds.UIA_ScrollVerticalScrollPercentPropertyId);
            Add("VerticalViewSize", UIA_PropertyIds.UIA_ScrollVerticalViewSizePropertyId);
            Add("VisualState", UIA_PropertyIds.UIA_WindowWindowVisualStatePropertyId);
        }
    }
}