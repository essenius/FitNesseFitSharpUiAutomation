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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using interop.UIAutomationCore;
using ImageHandler;
using UiAutomation.Model;
using Control = UiAutomation.Model.Control;

[assembly: CLSCompliant(true)]

namespace UiAutomation
{
    internal enum SearchType
    {
        Shallow,
        Deep
    }

    /// <summary>
    ///     FitSharp script table fixture code to test WPF and WinForm user interfaces.
    ///     This class represents the "View-Controller" region (i.e. getting the info from FitNesse,
    ///     handing off to the Model region, and returning the output delivered by the Model back to FitNesse
    /// </summary>
    /// <remarks>
    ///     Using the unmanaged API as that finds more than the managed API
    ///     see e.g. https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/c3f142e1-0624-4ec5-a313-482e72d5454d/
    ///     To make the constants work, set the reference property "Embed Interop Types" for interop.UIAutomationCore to False
    ///     The dll can be generated via:
    ///     "c:\Program Files(x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64\TlbImp.exe"
    ///     c:\Windows\System32\UIAutomationCore.dll -out:interop.UIAutomationCore.dll
    ///     Unless stated otherwise, design decisions have been made because they seemed reasonable.
    /// </remarks>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "FitSharp entry point"),
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used in FitSharp")]
    public class UiAutomationFixture
    {
        private bool _automaticSwitchToStartedApplication = true;
        private int _defaultTimeoutInDeciSeconds = 100;

        //private readonly SystemUnderTest _sut = new SystemUnderTest();
        private IApplication _sut;

        //private string _screenshotPath;
        private Control _window;

        /// <summary>
        ///     Documentation for FixtureExplorer
        /// </summary>
        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Script table interface for thick client (WinForms/WPF) testing via the UI Automation Framework"},
            {nameof(ApplicationProcessId), "The process Id of the currently active application under test"},
            {nameof(ClickControl), "Click a clickable control (e.g. Button)"},
            {
                nameof(CloseApplication),
                "Close a running application. It does so by closing the main window. It does not force a close if warnings or notifications pop up (e.g. 'are you sure?')"
            },
            {nameof(CollapseControl), "Expand a collapsible control (e.g. TreeItem)"},
            {nameof(ColumnCountOfControl), "Returns the number of columns in a grid control"},
            {nameof(ControlExists), "Returns whether a certain control exists"},
            {nameof(ControlIsVisible), "Returns whether a certain control is visible"},
            {nameof(DragControl), "Drag the mouse from a control. Use together with Drop On Control"},
            {nameof(DragControlAndDropOnControl), "Drag the mouse from a control and drop onto another control"},
            {nameof(DropOnControl), "Drop a dragged control onto another one. Use together with Drag Control"},
            {nameof(ExpandControl), "Expand an expandable control (e.g. TreeItem)"},
            {
                nameof(ForcedCloseApplication),
                "Close a running application by closing the main window. If the close does not succeed, it will try and kill the process (i.e. forced close)"
            },
            {nameof(IsUwpApp), "Returns whether the current application is an UWP app"},
            {nameof(NameOfControl), "Return the name of a control"},
            {
                nameof(NoAutomaticSwitchToStartedApplication),
                "If an application gets started, no automatic switch to the application window will be attempted. This will then need to be done manually with a Switch To Window command."
            },
            {
                nameof(PressKey),
                "Use the SendKeys.SendWait method to simulate keypresses. For more details on formats see http://msdn.microsoft.com/en-us/library/vstudio/system.windows.forms.sendkeys(v=vs.100).aspx"
            },
            {nameof(PropertyOf), "Returns a property of a control"},
            {nameof(RowCountOfControl), "Returns the number of rows in a grid control"},
            {nameof(RowNumberOfControlContaining), "Returns the row number of a control that contains a specific value of rows in a grid control "},
            {nameof(SearchBy), "Sets the default search method. If this command is not called, Name will be assumed"},
            {nameof(SelectItem), "Select a selectable item (e.g. RadioButton, Tab)"},
            {nameof(SetAutomaticSwitchToStartedApplication), "Enable automatic switching to an application that gets started (default setting)"},
            {nameof(SetFocusToControl), "Set the focus to a control"},
            {nameof(SetTimeoutSeconds), "Set the default timeout for all wait commands. Default value is 3 seconds"},
            {nameof(SetValueOfControlTo), "Set the value of a control. Tries to use an appropriate method based on the control type"},
            {nameof(SnapshotObjectOfControl), "Take a snapshot of a control on the screen and return it as a Snapshot object"},
            {nameof(SnapshotOfControl), "Take a snapshot of a control on the screen and render it as an HTML base 64 image"},
            {nameof(StartApplication), "Start an executable without arguments and default working folder"},
            {nameof(StartApplicationWithArguments), "Start an executable with arguments and default working folder"},
            {nameof(StartApplicationWithArgumentsAndWorkingFolder), "Start an executable with arguments and working folder"},
            {nameof(StartApplicationWithWorkingFolder), "Start an executable without arguments, with working folder"},
            {nameof(SwitchToParentWindow), "Switch to the parent window of the current app (useful for UWP apps)"},
            {nameof(SwitchToProcess), "Switch to a process (using either ProcessId or Name)"},
            {nameof(ToggleControl), "Toggles the value of a control"},
            {nameof(ValueOfControl), "Returns the value of a control. Tries to return an appropriate value based on the control type"},
            {nameof(WaitForControl), "Waits for a control to appear"},
            {nameof(WaitForControlAndClick), "Waits for a control to appear, and then click it"},
            {nameof(WaitForProcess), "Waits for a process to become active (typically via Name, can also use ProcessId"},
            {nameof(WaitUntilProcessEnds), "Waits for a process to end (via ProcessId or Name)"},
            {nameof(WaitUntilControlDisappears), "Wait for a control to disappear"},
            {nameof(WindowSnapshot), "Take a snapshot of the current window and render it as an HTML base 64 image"},
            {nameof(WindowSnapshotObject), "Take a snapshot of the current window and return it as a Snapshot object"}
        };

        public int ApplicationProcessId => _sut.ProcessId;

        private static Snapshot CaptureWindow(IUIAutomationElement element)
        {
            var rect = element.CurrentBoundingRectangle;
            var bounds = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            return Snapshot.CaptureScreen(bounds);
        }

        // todo: convert this into a query fixture or table fixture.
        public static string ListOfControlsFromRoot(string searchCriterion)
        {
            var controlList = Control.RootChildControls(searchCriterion);
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Found {0} items\n", controlList.Length);
            foreach (var control in controlList)
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, "Automation Id={0} Name={1} Value={2}", control.AutomationId,
                    control.Name, control.Value);
                var p = Mouse.AbsolutePosition(control.AutomationElement);
                sb.AppendFormat(CultureInfo.CurrentCulture, " (x:{0},y:{1})\n", p.X, p.Y);
            }

            return sb.ToString();
        }

        public static bool SearchBy(string conditionType)
        {
            Locator.DefaultConditionType = conditionType;
            // will only be set to a valid value
            return Locator.DefaultConditionType.Equals(conditionType, StringComparison.OrdinalIgnoreCase);
        }

        public static void WaitSeconds(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));

        private T ApplyMethodToControl<T>(Func<Control, T> methodToApply, string searchCriterion)
        {
            _sut?.WaitForInputIdle();
            var control = Control.FindControl(searchCriterion, _window);
            Debug.Assert(control != null, "Could not find " + searchCriterion);
            return methodToApply(control);
        }

        public bool ClickControl(string searchCriterion) => ApplyMethodToControl(x => x.Click(), searchCriterion);

        public bool CloseApplication()
        {
            const bool noForce = false;
            return _sut == null || _sut.Exit(noForce);
        }

        public bool CollapseControl(string searchCriterion) => ApplyMethodToControl(x => x.Collapse(), searchCriterion);

        public int ColumnCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.ColumnCount, searchCriterion);

        public bool ControlExists(string searchCriterion) => ApplyMethodToControl(x => x.Exists(), searchCriterion);

        public bool ControlIsVisible(string searchCriterion) => ApplyMethodToControl(x => x.IsVisible(), searchCriterion);

        public bool DragControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                Mouse.DragFrom(x.AutomationElement);
                return true;
            }, searchCriterion);
        }

        public bool DragControlAndDropOnControl(string dragCriterion, string dropCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                var y = ApplyMethodToControl(drop => drop, dropCriterion);
                Mouse.DragDrop(x.AutomationElement, y.AutomationElement);
                return true;
            }, dragCriterion);
        }

        public bool DropOnControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                Mouse.DropTo(x.AutomationElement);
                return true;
            }, searchCriterion);
        }

        public bool ExpandControl(string searchCriterion) => ApplyMethodToControl(x => x.Expand(), searchCriterion);

        public string FirstTextUnder(string searchCriterion) => ApplyMethodToControl(x => x.FirstTextUnder(), searchCriterion);

        public bool ForcedCloseApplication()
        {
            const bool force = true;
            return _sut == null || _sut.Exit(force, _defaultTimeoutInDeciSeconds * 100);
        }

        // testing purposes only
        internal Control GetControl(string locator)
        {
            var control = new Control(_window, SearchType.Deep, locator);
            control.FindControl();
            return control;
        }

        public bool IsUwpApp() => _sut != null && _sut.ApplicationType == "UWP";

        // For exploration of window contents
        public string ListOfControls(string searchCriterion)
        {
            if (_window == null || _sut == null) return string.Empty;
            var controlList = _window.DescendantControls(searchCriterion);
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Found {0} items\n", controlList.Length);
            foreach (var control in controlList)
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, "Automation Id={0}; Name={1}; Value={2}", control.AutomationId,
                    control.Name, control.Value);
                var p = Mouse.AbsolutePosition(control.AutomationElement);
                sb.AppendFormat(CultureInfo.CurrentCulture, " (x:{0},y:{1})\n", p.X, p.Y);
            }

            return sb.ToString();
        }

        public string NameOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Name, searchCriterion);

        public void NoAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = false;

        public bool PressKey(string key)
        {
            if (_window == null || _sut == null) return false;
            _sut.WaitForInputIdle();
            NativeMethods.SetForegroundWindow(_window.WindowHandle);
            Thread.Sleep(100);
            SendKeys.SendWait(key);
            Thread.Sleep(500); // needed to work around timing issues of SendWait
            return true;
        }

        public object PropertyOf(string property, string searchCriterion) => ApplyMethodToControl(x => x.Property(property), searchCriterion);

        public int RowCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.RowCount, searchCriterion);

        public string RowNumberOfControlContaining(string searchCriterion, string value) =>
            ApplyMethodToControl(x => x.RowNumberContaining(value), searchCriterion);

        public bool SelectItem(string searchCriterion) => ApplyMethodToControl(x => x.Select(), searchCriterion);

        public void SetAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = true;

        public bool SetFocusToControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                if (x.AutomationElement == null) return false;
                x.AutomationElement.SetFocus();
                return true;
            }, searchCriterion);
        }

        public int SetTimeoutSeconds(double timeout)
        {
            // don't think anyone wants to wait longer than a month - ever.
            const int maxTimeoutSeconds = 3600 * 24 * 31;
            _defaultTimeoutInDeciSeconds = timeout > 0 ? Convert.ToInt32(Math.Min(timeout, maxTimeoutSeconds) * 10) : 1;
            return _defaultTimeoutInDeciSeconds;
        }

        public bool SetValueOfControlTo(string searchCriterion, string newValue) => ApplyMethodToControl(x => x.SetValue(newValue), searchCriterion);

        public Snapshot SnapshotObjectOfControl(string locator) => ApplyMethodToControl(x => CaptureWindow(x.AutomationElement), locator);

        public string SnapshotOfControl(string locator) => SnapshotObjectOfControl(locator).Rendering;

        public bool StartApplication(string path) => StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, string.Empty);

        public bool StartApplicationWithArguments(string path, string arguments) =>
            StartApplicationWithArgumentsAndWorkingFolder(path, arguments, string.Empty);

        public bool StartApplicationWithArgumentsAndWorkingFolder(string path, string arguments, string workingFolder)
        {
            _sut = ApplicationFactory.Start(path, arguments, workingFolder);
            if (_sut == null) return false;
            if (!_sut.IsActive && _automaticSwitchToStartedApplication) return false;
            return !_automaticSwitchToStartedApplication || SwitchToAppWindow();
        }

        public bool StartApplicationWithWorkingFolder(string path, string workingFolder) =>
            StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, workingFolder);

        private bool SwitchTo(IApplication app)
        {
            if (app == null) return false;
            _sut = app;
            app.WaitForInputIdle();
            Debug.Print("ST:" + app.ProcessId);
            _window = app.WindowControl;
            if (_window == null) return false;
            var found = _window.WaitTillFound(_defaultTimeoutInDeciSeconds);
            return found;
        }

        internal bool SwitchToAppWindow() => SwitchTo(_sut);

        public bool SwitchToParentWindow()
        {
            var parent = _window?.Parent;
            return parent != null && SwitchToProcess("ProcessId:" + parent.CurrentProcessId);
        }

        public bool SwitchToProcess(string searchCriterion)
        {
            var processId = new ProcessHandler(searchCriterion).Id();
            Debug.WriteLine("ProcessId StP:" + processId);
            return processId != null && SwitchTo(ApplicationFactory.AttachToProcess(processId.Value));
        }

        private bool WaitForProcess(string searchCriterion, bool shouldbeAlive)
        {
            return searchCriterion.WaitWithTimeoutTill(criterion =>
            {
                var processId = new ProcessHandler(searchCriterion).Id();
                return shouldbeAlive ? processId != null : processId == null;
            }, _defaultTimeoutInDeciSeconds);
        }

        public bool WaitForProcess(string searchCriterion) => WaitForProcess(searchCriterion, true);

        public bool WaitUntilProcessEnds(string searchCriterion) => WaitForProcess(searchCriterion, false);

        [Obsolete("Use SwitchToProcess instead")]
        public bool SwitchToProcessById(int processId) => SwitchTo(ApplicationFactory.AttachToProcess(processId));

        // only works for classical applications. 
        [Obsolete("Use SwitchToProcess instead")]
        public bool SwitchToWindow(string locator)
        {
            var window = new Control(null, SearchType.Shallow, locator);
            return window.FindControl() && SwitchToProcessById(window.AutomationElement.CurrentProcessId);
        }

        [Obsolete("Use SwitchToProcess instead")]
        public bool SwitchToWindowByProcessName(string processName) => SwitchToProcess("name:" + processName);

        public bool ToggleControl(string searchCriterion) => ApplyMethodToControl(x => x.Toggle(), searchCriterion);

        public string ValueOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Value, searchCriterion);

        public bool WaitForControl(string searchCriterion) =>
            ApplyMethodToControl(x => x.WaitTillFound(_defaultTimeoutInDeciSeconds), searchCriterion);

        public bool WaitForControlAndClick(string searchCriterion) => WaitForControl(searchCriterion) && ClickControl(searchCriterion);

        public bool WaitUntilControlDisappears(string searchCriterion) =>
            ApplyMethodToControl(x => x.WaitTillNotFound(_defaultTimeoutInDeciSeconds), searchCriterion);

        public string WindowSnapshot() => WindowSnapshotObject().Rendering;

        public Snapshot WindowSnapshotObject() => _window == null ? null : CaptureWindow(_window.AutomationElement);
    }
}