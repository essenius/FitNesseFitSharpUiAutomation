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
using interop.UIAutomationCore;

namespace UiAutomation.Model
{
    internal class ControlTypeMapper : Mapping<int>
    {
        public ControlTypeMapper() : base("Control Type")
        {
            var initValues = new Dictionary<string, int>
            {
                { "Button", UIA_ControlTypeIds.UIA_ButtonControlTypeId }, //checked
                { "Calendar", UIA_ControlTypeIds.UIA_CalendarControlTypeId }, //checked
                { "CheckBox", UIA_ControlTypeIds.UIA_CheckBoxControlTypeId }, //checked
                { "ComboBox", UIA_ControlTypeIds.UIA_ComboBoxControlTypeId }, //checked
                { "CustomControl", UIA_ControlTypeIds.UIA_CustomControlTypeId },
                { "DataGrid", UIA_ControlTypeIds.UIA_DataGridControlTypeId },
                { "DataItem", UIA_ControlTypeIds.UIA_DataItemControlTypeId },
                { "Document", UIA_ControlTypeIds.UIA_DocumentControlTypeId },
                { "Edit", UIA_ControlTypeIds.UIA_EditControlTypeId }, // checked
                { "Group", UIA_ControlTypeIds.UIA_GroupControlTypeId },
                { "Header", UIA_ControlTypeIds.UIA_HeaderControlTypeId },
                { "HeaderItem", UIA_ControlTypeIds.UIA_HeaderItemControlTypeId },
                { "Hyperlink", UIA_ControlTypeIds.UIA_HyperlinkControlTypeId },
                { "Image", UIA_ControlTypeIds.UIA_ImageControlTypeId },
                { "List", UIA_ControlTypeIds.UIA_ListControlTypeId }, //checked
                { "ListItem", UIA_ControlTypeIds.UIA_ListItemControlTypeId },
                { "Menu", UIA_ControlTypeIds.UIA_MenuControlTypeId }, //checked
                { "MenuBar", UIA_ControlTypeIds.UIA_MenuBarControlTypeId },
                { "MenuItem", UIA_ControlTypeIds.UIA_MenuItemControlTypeId }, //checked
                { "Pane", UIA_ControlTypeIds.UIA_PaneControlTypeId },
                { "ProgressBar", UIA_ControlTypeIds.UIA_ProgressBarControlTypeId }, // checked
                { "RadioButton", UIA_ControlTypeIds.UIA_RadioButtonControlTypeId }, // checked
                { "ScrollBar", UIA_ControlTypeIds.UIA_ScrollBarControlTypeId }, // checked
                { "Separator", UIA_ControlTypeIds.UIA_SeparatorControlTypeId },
                { "Slider", UIA_ControlTypeIds.UIA_SliderControlTypeId }, // checked
                { "Spinner", UIA_ControlTypeIds.UIA_SpinnerControlTypeId },
                { "SplitButton", UIA_ControlTypeIds.UIA_SplitButtonControlTypeId },
                { "StatusBar", UIA_ControlTypeIds.UIA_StatusBarControlTypeId },
                { "Tab", UIA_ControlTypeIds.UIA_TabControlTypeId }, // checked
                { "TabItem", UIA_ControlTypeIds.UIA_TabItemControlTypeId }, // checked
                { "Table", UIA_ControlTypeIds.UIA_TableControlTypeId },
                { "Text", UIA_ControlTypeIds.UIA_TextControlTypeId },
                { "Thumb", UIA_ControlTypeIds.UIA_ThumbControlTypeId },
                { "TitleBar", UIA_ControlTypeIds.UIA_TitleBarControlTypeId },
                { "ToolBar", UIA_ControlTypeIds.UIA_ToolBarControlTypeId },
                { "ToolTip", UIA_ControlTypeIds.UIA_ToolTipControlTypeId },
                { "Tree", UIA_ControlTypeIds.UIA_TreeControlTypeId }, // checked
                { "TreeItem", UIA_ControlTypeIds.UIA_TreeItemControlTypeId }, // checked
                { "Window", UIA_ControlTypeIds.UIA_WindowControlTypeId },
                { "MainWindow", UIA_ControlTypeIds.UIA_WindowControlTypeId }
            };

            foreach (var line in initValues)
            {
                Add(line.Key, line.Value);
            }
        }

        public static bool IsMainWindow(string condition) => condition.Equals("MainWindow", StringComparison.OrdinalIgnoreCase);
    }
}
