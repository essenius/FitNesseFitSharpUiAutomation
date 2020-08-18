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

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using ImageHandler;
using interop.UIAutomationCore;

namespace UiAutomation.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "following Microsoft's naming of this assembly")]
    internal static class IUIAutomationElementExtensions
    {
        internal static Snapshot Capture(this IUIAutomationElement element, int border = 0)
        {
            var rect = element.CurrentBoundingRectangle;
            var bounds = new Rectangle(rect.left + border, rect.top + border,
                rect.right - rect.left - 2 * border, rect.bottom - rect.top - 2 * border);
            return Snapshot.CaptureScreen(bounds);
        }

        public static bool CollapseAll(this IUIAutomationElement element, IUIAutomation automation)
        {
            if (element.CurrentIsOffscreen != 0) return false;
            var returnValue = false;
            var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsEnabledPropertyId, true);
            var item = element.FindAll(TreeScope.TreeScope_Children, condition);
            for (var i = 0; i < item.Length; i++)
            {
                if (CollapseAll(item.GetElement(i), automation))
                {
                    returnValue = true;
                }
            }

            if (!(element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) is IUIAutomationExpandCollapsePattern
                    expandCollapsePattern) || expandCollapsePattern.CurrentExpandCollapseState ==
                ExpandCollapseState.ExpandCollapseState_LeafNode)
            {
                return returnValue;
            }
            expandCollapsePattern.Collapse();
            return true;
        }

        public static bool ExpandAll(this IUIAutomationElement element, IUIAutomation automation, int level)
        {
            if (element.CurrentIsOffscreen != 0) return false;
            var returnValue = false;
            if (element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) is IUIAutomationExpandCollapsePattern expandCollapsePattern &&
                expandCollapsePattern.CurrentExpandCollapseState != ExpandCollapseState.ExpandCollapseState_LeafNode)
            {
                returnValue = true;
                // TODO: find timing issue and resolve structurally. This issue shows with CalcVolume
                Thread.Sleep(100);
                expandCollapsePattern.Expand();
            }

            var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_IsEnabledPropertyId, true);
            var item = element.FindAll(TreeScope.TreeScope_Children, condition);
            for (var i = 0; i < item.Length; i++)
            {
                if (ExpandAll(item.GetElement(i), automation, level + 1)) returnValue = true;
            }

            return returnValue;
        }
    }
}
