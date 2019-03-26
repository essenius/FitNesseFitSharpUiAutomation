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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using ImageHandler;
using UiAutomation.Model;
using Control = UiAutomation.Model.Control;

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
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used in FitSharp"),
     Documentation("Script table interface for thick client (WinForms/WPF) testing via the UI Automation Framework")]
    public class UiAutomationFixture
    {
        private bool _automaticSwitchToStartedApplication = true;
        private BaseApplication _sut;
        private Control _window;

        [Documentation("The appliation under test is active")]
        public bool ApplicationIsActive => _sut?.IsActive ?? false;

        [Documentation("The process Id of the currently active application under test")]
        public int? ApplicationProcessId => _sut?.ProcessId;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"),
         SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local", Justification = "Used in unit test (PrivateType)")]
        private static Version PlatformVersion { get; set; } = Environment.OSVersion.Version;

        [Documentation("Set/get the default timeout for all wait commands. Default value is 3 seconds. Max is 3600 * 24 * 24 (i.e. 24 days)")]
        public static double TimeoutSeconds
        {
            get => ExtensionFunctions.TimeoutInMilliseconds / 1000.0;
            set
            {
                // We take this max because 24 full days of milliseconds just fits in an Int32, and Process.WaitForExit uses Int32.
                // Also, the minimal wait time is 100 milliseconds. We do this to ensure each wait function executes at least once.
                const int maxTimeoutSeconds = 3600 * 24 * 24;
                ExtensionFunctions.TimeoutInMilliseconds = value > 0 ? Convert.ToInt32(Math.Min(value, maxTimeoutSeconds) * 1000) : 100;
            }
        }

        [Documentation("Returns whether the platform supports UWP apps")]
        public static bool UwpAppsAreSupported => PlatformVersion.Major > 6 || PlatformVersion.Major == 6 && PlatformVersion.Minor >= 2;

        [Documentation("Height of the window of the system under test. 0 if nothing open")]
        public double WindowHeight => _window == null ? 0 : new Window(_window.AutomationElement).Height;

        [Documentation("Width of the window of the system under test. 0 if nothing open")]
        public double WindowWidth => _window == null ? 0 : new Window(_window.AutomationElement).Width;

        [Documentation("X position of the upper left corner of the window of the system under test. 0 if nothing open")]
        public double WindowX => _window == null ? 0 : new Window(_window.AutomationElement).Left;

        [Documentation("Y position of the upper left corner of the window of the system under test. 0 if nothing open")]
        public double WindowY => _window == null ? 0 : new Window(_window.AutomationElement).Top;

        private T ApplyMethodToControl<T>(Func<Control, T> methodToApply, string searchCriterion)
        {
            _sut?.WaitForInputIdle();
            var control = Control.FindControl(searchCriterion, _window);
            Debug.Assert(control != null, "Could not find " + searchCriterion);
            return methodToApply(control);
        }

        [Documentation("Click a clickable control (e.g. Button)")]
        public bool ClickControl(string searchCriterion) => ApplyMethodToControl(x => x.Click(), searchCriterion);

        [Documentation("Close a running application. It does so by closing the main window. " +
                       "It does not force a close if warnings or notifications pop up (e.g. 'are you sure?')")]
        public bool CloseApplication()
        {
            const bool noForce = false;
            return _sut == null || _sut.Exit(noForce);
        }

        [Documentation("Collapse a collapsible control (e.g. TreeItem)")]
        public bool CollapseControl(string searchCriterion) => ApplyMethodToControl(x => x.Collapse(), searchCriterion);

        [Documentation("Returns the number of columns in a grid control")]
        public int ColumnCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.ColumnCount, searchCriterion);

        [Documentation("Returns whether a certain control exists")]
        public bool ControlExists(string searchCriterion) => ApplyMethodToControl(x => x.Exists(), searchCriterion);

        [Documentation("Returns whether a certain control is visible")]
        public bool ControlIsVisible(string searchCriterion) => ApplyMethodToControl(x => x.IsVisible(), searchCriterion);

        [Documentation("Drag the mouse from a control. Use together with Drop On Control")]
        public bool DragControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                Mouse.DragFrom(x.AutomationElement);
                return true;
            }, searchCriterion);
        }

        [Documentation("Drag the mouse from a control and drop onto another control")]
        public bool DragControlAndDropOnControl(string dragCriterion, string dropCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                var y = ApplyMethodToControl(drop => drop, dropCriterion);
                Mouse.DragDrop(x.AutomationElement, y.AutomationElement);
                return true;
            }, dragCriterion);
        }

        [Documentation("Drop a dragged control onto another one. Use together with Drag Control")]
        public bool DropOnControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                Mouse.DropTo(x.AutomationElement);
                return true;
            }, searchCriterion);
        }

        [Documentation("Expand an expandable control (e.g. TreeItem)")]
        public bool ExpandControl(string searchCriterion) => ApplyMethodToControl(x => x.Expand(), searchCriterion);

        [Documentation("Return the content of the first text control under the current control. Useful for UWP apps")]
        public string FirstTextUnder(string searchCriterion) => ApplyMethodToControl(x => x.FirstTextUnder(), searchCriterion);

        [Documentation("Close a running application by closing the main window. " +
                       "If the close does not succeed, it will try and kill the process (i.e. forced close)")]
        public bool ForcedCloseApplication()
        {
            const bool force = true;
            return _sut == null || _sut.Exit(force);
        }

        internal Control GetControl(string locator)
        {
            var control = new Control(_window, SearchType.Deep, locator);
            control.FindControl();
            return control;
        }

        [Documentation("Returns whether the current application is an UWP app")]
        public bool IsUwpApp() => _sut != null && _sut.ApplicationType == "UWP";

        [Documentation("Maximize the window of the system under test")]
        public bool MaximizeWindow() => new Window(_window?.AutomationElement).Maximize();

        [Documentation("Minimize the window of the system under test")]
        public bool MinimizeWindow() => new Window(_window?.AutomationElement).Minimize();

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x"),
         SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y"),
         Documentation("Move a window to a certain x and y position")]
        public bool MoveWindow(int x, int y)
        {
            if (_window == null || _sut == null) return false;
            _sut.WaitForInputIdle();
            return new Window(_window.AutomationElement).Move(x, y);
        }

        [Documentation("Return the name of a control")]
        public string NameOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Name, searchCriterion);

        [Documentation("If an application gets started, no automatic switch to the application window will be attempted. " +
                       "This will then need to be done manually with a Switch To Window command.")]
        public void NoAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = false;

        [Documentation("'Restore' the window od the system under test")]
        public bool NormalWindow() => new Window(_window?.AutomationElement).Normal();

        [Documentation("Use the SendKeys.SendWait method to simulate keypresses. " +
                       "For more details on formats see https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys")]
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

        [Documentation("Returns a property of a control")]
        public object PropertyOf(string property, string searchCriterion) => ApplyMethodToControl(x => x.Property(property), searchCriterion);

        [Documentation("Resize a window to a certain width and height")]
        public bool ResizeWindow(int width, int height)
        {
            if (_window == null || _sut == null) return false;
            _sut.WaitForInputIdle();
            return new Window(_window.AutomationElement).Resize(width, height);
        }

        [Documentation("Returns the number of rows in a grid control")]
        public int RowCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.RowCount, searchCriterion);

        [Documentation("Returns the row number of a control that contains a specific value of rows in a grid control")]
        public string RowNumberOfControlContaining(string searchCriterion, string value) =>
            ApplyMethodToControl(x => x.RowNumberContaining(value), searchCriterion);

        [Documentation("Sets the default search method. If this command is not called, Name will be assumed")]
        public static bool SearchBy(string conditionType)
        {
            Locator.DefaultConditionType = conditionType;
            // will only be set to a valid value
            return Locator.DefaultConditionType.Equals(conditionType, StringComparison.OrdinalIgnoreCase);
        }

        [Documentation("Select a selectable item (e.g. RadioButton, Tab)")]
        public bool SelectItem(string searchCriterion) => ApplyMethodToControl(x => x.Select(), searchCriterion);

        [Documentation("Enable automatic switching to an application that gets started (default setting)")]
        public void SetAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = true;

        [Documentation("Set the focus to a control")]
        public bool SetFocusToControl(string searchCriterion)
        {
            return ApplyMethodToControl(x =>
            {
                if (x.AutomationElement == null) return false;
                x.AutomationElement.SetFocus();
                return true;
            }, searchCriterion);
        }

        [Documentation("Set the value of a control. Tries to use an appropriate method based on the control type")]
        public bool SetValueOfControlTo(string searchCriterion, string newValue) => ApplyMethodToControl(x => x.SetValue(newValue), searchCriterion);

        [Documentation("Take a snapshot of a control on the screen and return it as a Snapshot object")]
        public Snapshot SnapshotObjectOfControl(string locator) => ApplyMethodToControl(x => x.AutomationElement.Capture(), locator);

        [Documentation("Take a snapshot of a control on the screen and render it as an HTML base 64 image")]
        public string SnapshotOfControl(string locator) => SnapshotObjectOfControl(locator).Rendering;

        [Documentation("Start an executable without arguments and default working folder")]
        public bool StartApplication(string path) => StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, string.Empty);

        [Documentation("Start an executable with arguments and default working folder")]
        public bool StartApplicationWithArguments(string path, string arguments) =>
            StartApplicationWithArgumentsAndWorkingFolder(path, arguments, string.Empty);

        [Documentation("Start an executable with arguments and working folder")]
        public bool StartApplicationWithArgumentsAndWorkingFolder(string path, string arguments, string workingFolder)
        {
            _sut = ApplicationFactory.Start(path, arguments, workingFolder);
            if (_sut == null) return false;
            if (!_sut.IsActive && _automaticSwitchToStartedApplication) return false;
            return !_automaticSwitchToStartedApplication || SwitchToAppWindow();
        }

        [Documentation("Start an executable without arguments, with working folder")]
        public bool StartApplicationWithWorkingFolder(string path, string workingFolder) =>
            StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, workingFolder);

        private bool SwitchTo(BaseApplication app)
        {
            if (app == null) return false;
            _sut = app;
            //The app level wait should already cover exceptions, so no need to check them here
            app.WaitForInputIdle();

            _window = app.WindowControl;
            if (_window == null) return false;
            // This is needed to populate the control's AutomationElement
            return _window.WaitTillFound();
        }

        internal bool SwitchToAppWindow() => SwitchTo(_sut);

        [Documentation("Switch to the parent window of the current app (useful for UWP apps)")]
        public bool SwitchToParentWindow()
        {
            var parent = _window?.Parent;
            return parent != null && SwitchToProcess("ProcessId:" + parent.CurrentProcessId);
        }

        [Documentation("Switch to a process (using either ProcessId or Name)")]
        public bool SwitchToProcess(string searchCriterion)
        {
            var processId = new ProcessHandler(searchCriterion).Id();
            return processId != null && SwitchTo(ApplicationFactory.AttachToProcess(processId.Value));
        }

        [Obsolete("Use SwitchToProcess instead")]
        public bool SwitchToProcessById(int processId) => SwitchTo(ApplicationFactory.AttachToProcess(processId));

        [Obsolete("Only works for classical applications. Use SwitchToProcess instead")]
        public bool SwitchToWindow(string locator)
        {
            var window = new Control(null, SearchType.Shallow, locator);
            return window.FindControl() && SwitchToProcessById(window.AutomationElement.CurrentProcessId);
        }

        [Obsolete("Use SwitchToProcess instead")]
        public bool SwitchToWindowByProcessName(string processName) => SwitchToProcess("name:" + processName);

        [Documentation("Toggles the value of a control")]
        public bool ToggleControl(string searchCriterion) => ApplyMethodToControl(x => x.Toggle(), searchCriterion);

        [Documentation("Returns the value of a control. Tries to return an appropriate value based on the control type")]
        public string ValueOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Value, searchCriterion);

        [Documentation("Waits for a control to appear")]
        public bool WaitForControl(string searchCriterion) =>
            ApplyMethodToControl(x => x.WaitTillFound(), searchCriterion);

        [Documentation("Waits for a control to appear, and then click it")]
        public bool WaitForControlAndClick(string searchCriterion) => WaitForControl(searchCriterion) && ClickControl(searchCriterion);

        private static bool WaitForProcess(string searchCriterion, bool shouldbeAlive)
        {
            return searchCriterion.WaitWithTimeoutTill(criterion =>
            {
                var processId = new ProcessHandler(searchCriterion).Id();
                return shouldbeAlive ? processId != null : processId == null;
            });
        }

        [Documentation("Waits for a process to become active (typically via Name, can also use ProcessId")]
        public static bool WaitForProcess(string searchCriterion) => WaitForProcess(searchCriterion, true);

        [Documentation("Wait the specified number of seconds (can be fractions)")]
        public static void WaitSeconds(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));

        [Documentation("Wait for a control to disappear")]
        public bool WaitUntilControlDisappears(string searchCriterion) =>
            ApplyMethodToControl(x => x.WaitTillNotFound(), searchCriterion);

        [Documentation("Waits for a process to end (via ProcessId or Name)")]
        public static bool WaitUntilProcessEnds(string searchCriterion) => WaitForProcess(searchCriterion, false);

        [Documentation("Take a snapshot of the current window and render it as an HTML base 64 image")]
        public string WindowSnapshot() => WindowSnapshot(0);

        [Documentation("Take a snapshot of the current window removing a border width and render it as an HTML base 64 image")]
        public string WindowSnapshot(int border) => WindowSnapshotObject(border).Rendering;

        [Documentation("Take a snapshot of the current window and return it as a Snapshot object")]
        public Snapshot WindowSnapshotObject() => WindowSnapshotObject(0);

        [Documentation("Take a snapshot of the current window removing a border width and return it as a Snapshot object")]
        public Snapshot WindowSnapshotObject(int border)
        {
            NativeMethods.SetForegroundWindow(_window.WindowHandle);
            new Window(_window.AutomationElement).WaitTillOnScreen();
            return _window.AutomationElement.Capture(border);
        }
    }
}