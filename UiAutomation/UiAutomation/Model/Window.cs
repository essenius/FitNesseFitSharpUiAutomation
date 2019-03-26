// Copyright 2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics;
using interop.UIAutomationCore;

namespace UiAutomation.Model
{
    internal class Window
    {
        private readonly IUIAutomationTransformPattern _transformPattern;
        private readonly IUIAutomationElement _window;
        private IUIAutomationWindowPattern _windowPattern;

        public Window(IUIAutomationElement window)
        {
            _window = window;
            _transformPattern = window?.GetCurrentPattern(UIA_PatternIds.UIA_TransformPatternId) as IUIAutomationTransformPattern;
        }

        private tagRECT BoundingRectangle => _window.CurrentBoundingRectangle;

        public double Height => BoundingRectangle.bottom - BoundingRectangle.top;

        public double Left => BoundingRectangle.left;

        public double Top => BoundingRectangle.top;

        public double Width => BoundingRectangle.right - BoundingRectangle.left;

        public bool IsTopmost()
        {
            _windowPattern = _window?.GetCurrentPattern(UIA_PatternIds.UIA_WindowPatternId) as IUIAutomationWindowPattern;
            if (_windowPattern == null) return false;
            return _windowPattern.CurrentIsTopmost != 0;
        }

        public bool Maximize() => SetWindowVisualState(WindowVisualState.WindowVisualState_Maximized);
        public bool Minimize() => SetWindowVisualState(WindowVisualState.WindowVisualState_Minimized);

        public bool Move(int xCoordinate, int yCoordinate)
        {
            if (_transformPattern == null) return false;
            _transformPattern.Move(xCoordinate, yCoordinate);
            return true;
        }

        public bool Normal() => SetWindowVisualState(WindowVisualState.WindowVisualState_Normal);

        public bool Resize(int width, int height)
        {
            if (_transformPattern == null) return false;
            _transformPattern.Resize(width, height);
            return true;
        }

        private bool SetWindowVisualState(WindowVisualState state)
        {
            _windowPattern = _window?.GetCurrentPattern(UIA_PatternIds.UIA_WindowPatternId) as IUIAutomationWindowPattern;
            if (_windowPattern == null) return false;
            _windowPattern.SetWindowVisualState(state);
            _windowPattern.WaitWithTimeoutTill(x => x.CurrentWindowVisualState == state);
            return _windowPattern.CurrentWindowVisualState == state;
        }

        public bool WaitTillOnScreen()
        {
            return this.WaitWithTimeoutTill(x => x.IsTopmost());
        }
    }
}