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

using System.Drawing;
using interop.UIAutomationCore;

namespace UiAutomation.Model;

internal static class Mouse
{
    // this stuff is pretty tricky. You can't use IUIAutomationElement calls like CurrentBoundingRectangle and FindElement
    // after you have started dragging - that can freeze the application until you move the mouse. 
    // so we need to ensure that we have the absolute coordinates of the drag/drop before we start the drag/drop process.
    // To work around that we remember the drag point, and do the whole drag and drop process in the DropTo method.
    // still need to figure out how to solve this structurally - expect something around threading.

    private static Point? _dragFromPoint;
    private static readonly int ScreenHeight = NativeMethods.GetSystemMetrics(1);
    private static readonly int ScreenWidth = NativeMethods.GetSystemMetrics(0);

    public static Point AbsolutePosition(IUIAutomationElement element)
    {
        var rectangle = element.CurrentBoundingRectangle;
        var midX = (rectangle.left + rectangle.right) / 2;
        var midY = (rectangle.top + rectangle.bottom) / 2;
        var absX = midX * 65536 / ScreenWidth;
        var absY = midY * 65536 / ScreenHeight;
        return new Point(absX, absY);
    }

    public static bool Click(IUIAutomationElement element)
    {
        {
            var clickPoint = AbsolutePosition(element);

            if (!MouseEvent(
                    NativeMethods.MouseEventFlags.Move | NativeMethods.MouseEventFlags.Absolute,
                    clickPoint.X,
                    clickPoint.Y))
            {
                return false;
            }

            return MouseEvent(NativeMethods.MouseEventFlags.LeftDown, 0, 0) &&
                   MouseEvent(NativeMethods.MouseEventFlags.LeftUp, 0, 0);
        }
    }

    public static bool DoubleClick(IUIAutomationElement element) => Click(element) && Click(element);

    public static bool DragDrop(IUIAutomationElement dragFrom, IUIAutomationElement dropTo) =>
        DragFrom(dragFrom) && DropTo(dropTo);

    public static bool DragFrom(IUIAutomationElement element)
    {
        _dragFromPoint = AbsolutePosition(element);
        return true;
    }

    public static bool DropTo(IUIAutomationElement element)
    {
        if (!_dragFromPoint.HasValue) return false;
        var dropToPoint = AbsolutePosition(element);
        if (!MoveToAndDo(_dragFromPoint.Value, NativeMethods.MouseEventFlags.LeftDown)) return false;
        if (!MoveToAndDo(dropToPoint, NativeMethods.MouseEventFlags.LeftUp)) return false;
        _dragFromPoint = null;
        return true;
    }

    private static bool MouseEvent(NativeMethods.MouseEventFlags eventFlags, int x, int y)
    {
        var mouseInput = new[]
        {
            new NativeMethods.Input
            {
                type = NativeMethods.SendInputEventType.InputMouse,
                U = new NativeMethods.InputUnion
                {
                    mi = new NativeMethods.MouseInput
                    {
                        x = x,
                        y = y,
                        mouseData = 0,
                        flags = (uint)eventFlags
                    }
                }
            }
        };

        var returnValue = NativeMethods.SendInput(1, mouseInput, NativeMethods.Input.Size);

        return returnValue == 1;
    }

    private static bool MoveToAndDo(Point position, NativeMethods.MouseEventFlags eventFlags) =>
        MouseEvent(
            NativeMethods.MouseEventFlags.Absolute | NativeMethods.MouseEventFlags.Move,
            position.X,
            position.Y) &&
        MouseEvent(eventFlags, 0, 0);
}