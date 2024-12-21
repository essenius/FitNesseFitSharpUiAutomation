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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using ImageHandler;
using UiAutomation.Model;
using Control = UiAutomation.Model.Control;

namespace UiAutomation;

internal enum SearchType
{
    Shallow,
    Deep
}

/// <summary>
///     Script table interface for thick client (WinForms/WPF) testing via the UI Automation Framework.
/// </summary>
/// <remarks>
///     Using the unmanaged API as that finds more than the managed API
///     see e.g. https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/c3f142e1-0624-4ec5-a313-482e72d5454d/
///     To make the constants work, set the reference property "Embed Interop Types" for interop.UIAutomationCore to False
///     The dll can be generated via:
///     "c:\Program Files(x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64\TlbImp.exe"
///     c:\Windows\System32\UIAutomationCore.dll -out:interop.UIAutomationCore.dll
///     Unless stated otherwise, design decisions have been made because they seemed reasonable.
/// </remarks>
public class UiAutomationFixture
{
    private bool _automaticSwitchToStartedApplication = true;
    private BaseApplication _sut;
    private Control _window;

    /// <summary>Is the application under test active?</summary>
    public bool ApplicationIsActive => _sut?.IsActive ?? false;

    /// <summary>The process Id of the currently active application under test</summary>
    public int? ApplicationProcessId => _sut?.ProcessId;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local -- used via reflection in FixtureTest
    private static Version PlatformVersion { get; set; } = Environment.OSVersion.Version;

    /// <summary>
    ///     Set/get the default timeout for all wait commands. Default value is 3 seconds. Max is 3600 * 24 * 24 (i.e. 24
    ///     days)
    /// </summary>
    public static double TimeoutSeconds
    {
        get => ExtensionFunctions.TimeoutInMilliseconds / 1000.0;
        set
        {
            // We take this max because 24 full days of milliseconds just fits in an Int32, and Process.WaitForExit uses Int32.
            // Also, the minimal wait time is 100 milliseconds. We do this to ensure each wait function executes at least once.
            const int maxTimeoutSeconds = 3600 * 24 * 24;
            ExtensionFunctions.TimeoutInMilliseconds =
                value > 0 ? Convert.ToInt32(Math.Min(value, maxTimeoutSeconds) * 1000) : 100;
        }
    }

    /// <summary>Returns whether the platform supports UWP apps</summary>
    public static bool UwpAppsAreSupported => PlatformVersion.Major > 6 || (PlatformVersion.Major == 6 && PlatformVersion.Minor >= 2);

    /// <summary>Window size (width, height) of the system under test, in pixels. 0,0 if nothing open</summary>
    public Coordinate WindowSize => _window == null ? new Coordinate(0, 0) : new Window(_window.AutomationElement).Size;

    /// <summary>Coordinate (X,Y) of the top left corner of the system under test's window, in pixels. 0,0 if nothing open</summary>
    public Coordinate WindowTopLeft => _window == null ? new Coordinate(0, 0) : new Window(_window.AutomationElement).TopLeft;

    private T ApplyMethodToControl<T>(Func<Control, T> methodToApply, string searchCriterion)
    {
        _sut?.WaitForInputIdle();
        var control = Control.FindControl(searchCriterion, _window);
        Debug.Assert(control != null, "Could not find " + searchCriterion);
        return methodToApply(control);
    }

    /// <summary>Get the row and column of the first cell in a grid that contains the value</summary>
    public GridItem CellInControlContaining(string searchCriterion, string value) =>
        ApplyMethodToControl(x => x.CellContaining(value), searchCriterion);

    /// <summary>Click a clickable control (e.g. Button)</summary>
    public bool ClickControl(string searchCriterion) => ApplyMethodToControl(x => x.Click(), searchCriterion);

    /// <summary>
    ///     Close a running application. It does so by closing the main window. It does not force a close if warnings or
    ///     notifications pop up (e.g. 'are you sure?')
    /// </summary>
    public bool CloseApplication()
    {
        const bool noForce = false;
        return _sut == null || _sut.Exit(noForce);
    }

    /// <summary>Collapse a collapsible control (e.g. TreeItem)</summary>
    public bool CollapseControl(string searchCriterion) => ApplyMethodToControl(x => x.Collapse(), searchCriterion);

    /// <summary>Returns the number of columns in a grid control</summary>
    public int ColumnCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.ColumnCount, searchCriterion);

    /// <summary>Returns whether a certain control exists</summary>
    public bool ControlExists(string searchCriterion) => ApplyMethodToControl(x => x.Exists(), searchCriterion);

    /// <summary>Returns whether a certain control is visible</summary>
    public bool ControlIsVisible(string searchCriterion) => ApplyMethodToControl(x => x.IsVisible(), searchCriterion);

    /// <summary>Double click a control</summary>
    public bool DoubleClickControl(string searchCriterion) => ApplyMethodToControl(x => x.DoubleClick(), searchCriterion);

    /// <summary>Drag the mouse from a control. Use together with Drop On Control</summary>
    public bool DragControl(string searchCriterion) => ApplyMethodToControl(x =>
        {
            Mouse.DragFrom(x.AutomationElement);
            return true;
        }, searchCriterion
    );

    /// <summary>Drag the mouse from a control and drop onto another control</summary>
    public bool DragControlAndDropOnControl(string dragCriterion, string dropCriterion) => ApplyMethodToControl(x =>
        {
            var y = ApplyMethodToControl(drop => drop, dropCriterion);
            return Mouse.DragDrop(x.AutomationElement, y.AutomationElement);
        }, dragCriterion
    );

    /// <summary>Drop a dragged control onto another one. Use together with Drag Control</summary>
    public bool DropOnControl(string searchCriterion) => ApplyMethodToControl(x => Mouse.DropTo(x.AutomationElement), searchCriterion);

    /// <summary>Expand an expandable control (e.g. TreeItem)</summary>
    public bool ExpandControl(string searchCriterion) => ApplyMethodToControl(x => x.Expand(), searchCriterion);

    /// <summary>Return the content of the first text control under the current control. Useful for UWP apps</summary>
    public string FirstTextUnder(string searchCriterion) => ApplyMethodToControl(x => x.FirstTextUnder(), searchCriterion);

    /// <summary>
    ///     Close a running application by closing the main window. If the close does not succeed, it will try and kill the
    ///     process
    ///     (i.e. forced close)
    /// </summary>
    public bool ForcedCloseApplication()
    {
        const bool force = true;
        return _sut == null || _sut.Exit(force);
    }

    internal Control GetControl(string locator)
    {
        var control = new Control(locator) { SearchType = SearchType.Deep, Parent = _window };
        control.FindControl();
        return control;
    }

    /// <summary>Returns whether the current application is an UWP app</summary>
    public bool IsUwpApp() => _sut is { ApplicationType: "UWP" };

    /// <summary>Maximize the window of the system under test</summary>
    public bool MaximizeWindow() => new Window(_window?.AutomationElement).Maximize();

    /// <summary>Minimize the window of the system under test</summary>
    public bool MinimizeWindow() => new Window(_window?.AutomationElement).Minimize();

    /// <summary>Move a window to a certain x and y position</summary>
    public bool MoveWindowTo(Coordinate coordinate)
    {
        if (_window == null || _sut == null || coordinate == null) return false;
        _sut.WaitForInputIdle();
        return new Window(_window.AutomationElement).Move(coordinate.X, coordinate.Y);
    }

    /// <summary>Return the name of a control</summary>
    public string NameOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Name, searchCriterion);

    /// <summary>
    ///     If an application gets started, no automatic switch to the application window will be attempted. This will then
    ///     need to
    ///     be done manually with a Switch To Window command
    /// </summary>
    public void NoAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = false;

    /// <summary>'Restore' the window of the system under test</summary>
    public bool NormalWindow() => new Window(_window?.AutomationElement).Normal();

    /// <summary>Use the SendKeys.SendWait method to simulate keypresses.</summary>
    /// <param name="key">
    ///     For more details on formats see
    ///     https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys
    /// </param>
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

    /// <summary>Use the SendKeys.SendWait method to simulate keypresses. Synonym for PressKey</summary>
    /// <param name="key">
    ///     For more details on formats see
    ///     https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys
    /// </param>
    public bool PressKeys(string key) => PressKey(key);

    /// <summary>Returns a property of a control</summary>
    public object PropertyOfControl(string property, string searchCriterion) => ApplyMethodToControl(x => x.Property(property), searchCriterion);

    /// <summary>Resize a window to a certain width and height. Format: width, height</summary>
    public bool ResizeWindowTo(Coordinate size)
    {
        if (_window == null || _sut == null || size == null) return false;
        _sut.WaitForInputIdle();
        return new Window(_window.AutomationElement).Resize(size.X, size.Y);
    }

    /// <summary>Returns the number of rows in a grid control</summary>
    public int RowCountOfControl(string searchCriterion) => ApplyMethodToControl(x => x.RowCount, searchCriterion);

    /// <summary>Sets the default search method. If this command is not called, Name will be assumed</summary>
    public static bool SearchBy(string conditionType)
    {
        Locator.DefaultConditionType = conditionType;
        // will only be set to a valid value
        return Locator.DefaultConditionType.Equals(conditionType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Get the row and column of the selected cell in a control. If multiple are selected, it returns the first only</summary>
    public GridItem SelectedCellInControl(string searchCriterion) => ApplyMethodToControl(x => x.SelectedCell(), searchCriterion);

    /// <summary>
    ///     returns the selected items. If none are selected, it returns null. If one is selected, it returns the name.
    ///     If multiple are selected, it returns a list of names between brackets, separated by comma and space
    /// </summary>
    public string SelectedItems(string searchCriterion) => ApplyMethodToControl(x =>
        {
            var result = x.SelectedItems();
            switch (result.Length)
            {
                case 0:
                    return null;
                case 1:
                    return result.GetElement(0).CurrentName;
                default:
                {
                    var strings = new List<string>();
                    for (var i = 0; i < result.Length; i++)
                    {
                        strings.Add(result.GetElement(0).CurrentName);
                    }

                    return "[" + string.Join(", ", strings) + "]";
                }
            }
        }, searchCriterion
    );

    /// <summary>Select a selectable item (e.g. RadioButton, Tab)</summary>
    public bool SelectItem(string searchCriterion) => ApplyMethodToControl(x => x.Select(), searchCriterion);

    /// <summary>Enable automatic switching to an application that gets started (default setting)</summary>
    public void SetAutomaticSwitchToStartedApplication() => _automaticSwitchToStartedApplication = true;

    /// <summary>Set the focus to a control</summary>
    public bool SetFocusToControl(string searchCriterion) => ApplyMethodToControl(x =>
        {
            if (x.AutomationElement == null) return false;
            x.AutomationElement.SetFocus();
            return true;
        }, searchCriterion
    );

    /// <summary>Set the value of a control. Tries to use an appropriate method based on the control type</summary>
    public bool SetValueOfControlTo(string searchCriterion, string newValue) => ApplyMethodToControl(x => x.SetValue(newValue), searchCriterion);

    /// <summary>Take a snapshot of a control on the screen and return it as a Snapshot object</summary>
    public Snapshot SnapshotObjectOfControl(string locator) => ApplyMethodToControl(x => x.AutomationElement.Capture(), locator);

    /// <summary>Take a snapshot of a control on the screen and render it as an HTML base 64 image</summary>
    public string SnapshotOfControl(string locator) => SnapshotObjectOfControl(locator).Rendering;

    /// <summary>Start an executable without arguments and default working folder</summary>
    public bool StartApplication(string path) => StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, string.Empty);

    /// <summary>Start an executable with arguments and default working folder</summary>
    public bool StartApplicationWithArguments(string path, string arguments) =>
        StartApplicationWithArgumentsAndWorkingFolder(path, arguments, string.Empty);

    /// <summary>Start an executable with arguments and working folder</summary>
    public bool StartApplicationWithArgumentsAndWorkingFolder(string path, string arguments, string workingFolder)
    {
        _sut = ApplicationFactory.Start(path, arguments, workingFolder);
        if (_sut == null) return false;
        if (!_sut.IsActive && _automaticSwitchToStartedApplication) return false;
        return !_automaticSwitchToStartedApplication || SwitchToAppWindow();
    }

    /// <summary>Start an executable without arguments, with working folder</summary>
    public bool StartApplicationWithWorkingFolder(string path, string workingFolder) =>
        StartApplicationWithArgumentsAndWorkingFolder(path, string.Empty, workingFolder);

    private bool SwitchTo(BaseApplication app)
    {
        if (app == null) return false;
        _sut = app;
        //The app level wait should already cover exceptions, so no need to check them here
        app.WaitForInputIdle();
        if (!app.WaitForMainWindow()) return false;
        // it may take a while till the window is fully loaded
        _ = false.WaitWithTimeoutTill(_ =>
        {
            _window = app.WindowControl;
            return _window != null;
        });
        // WaitTillFound is needed to populate the control's AutomationElement
        return _window != null && _window.WaitTillFound();
    }

    internal bool SwitchToAppWindow() => SwitchTo(_sut);

    /// <summary>Switch to the parent window of the current app (useful for UWP apps)</summary>
    public bool SwitchToParentWindow()
    {
        var result = false.WaitWithTimeoutTill(_ =>
            {
                var parent = _window?.FindParentElement;
                return parent != null && SwitchToProcess($"ProcessId:{parent.CurrentProcessId}");
            }
        );
        return result;
    }

    /// <summary>Switch to a process (using either ProcessId or Name)</summary>
    public bool SwitchToProcess(string searchCriterion)
    {
        var processId = new ProcessHandler(searchCriterion).Id();
        var returnValue = processId != null && SwitchTo(ApplicationFactory.AttachToProcess(processId.Value));
        return returnValue;
    }

    /// <summary>Toggles the value of a control</summary>
    public bool ToggleControl(string searchCriterion) => ApplyMethodToControl(x => x.Toggle(), searchCriterion);

    /// <summary>Returns the value of a control. Tries to return an appropriate value based on the control type</summary>
    public string ValueOfControl(string searchCriterion) => ApplyMethodToControl(x => x.Value, searchCriterion);

    /// <summary>Waits for a control to appear</summary>
    public bool WaitForControl(string searchCriterion) => ApplyMethodToControl(x => x.WaitTillFound(), searchCriterion);

    /// <summary>Waits for a control to appear, and then click it</summary>
    public bool WaitForControlAndClick(string searchCriterion) => WaitForControl(searchCriterion) && ClickControl(searchCriterion);

    private static bool WaitForProcess(string searchCriterion, bool shouldbeAlive) => searchCriterion.WaitWithTimeoutTill(_ =>
        {
            var processId = new ProcessHandler(searchCriterion).Id();
            return shouldbeAlive ? processId != null : processId == null;
        }
    );

    /// <summary>Waits for a process to become active (typically via Name, can also use ProcessId</summary>
    public static bool WaitForProcess(string searchCriterion) => WaitForProcess(searchCriterion, true);

    /// <summary>Wait the specified number of seconds (can be fractions)</summary>
    public static void WaitSeconds(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));

    /// <summary>Wait for a control to disappear</summary>
    public bool WaitUntilControlDisappears(string searchCriterion) => ApplyMethodToControl(x => x.WaitTillNotFound(), searchCriterion);

    /// <summary>Waits for a process to end (via ProcessId or Name)</summary>
    public static bool WaitUntilProcessEnds(string searchCriterion) => WaitForProcess(searchCriterion, false);

    /// <summary>Take a snapshot of the current window and render it as an HTML base 64 image</summary>
    public string WindowSnapshot() => WindowSnapshotObject().Rendering;

    /// <summary>Take a snapshot of the current window removing a border width and render it as an HTML base 64 image</summary>
    public string WindowSnapshotMinusOuterPixels(int border) => WindowSnapshotObjectMinusOuterPixels(border).Rendering;

    /// <summary>Take a snapshot of the current window and return it as a Snapshot object</summary>
    public Snapshot WindowSnapshotObject() => WindowSnapshotObjectMinusOuterPixels(0);

    /// <summary>Take a snapshot of the current window removing a border width and return it as a Snapshot object</summary>
    public Snapshot WindowSnapshotObjectMinusOuterPixels(int border)
    {
        NativeMethods.SetForegroundWindow(_window.WindowHandle);
        new Window(_window.AutomationElement).WaitTillOnScreen();
        return _window.AutomationElement.Capture(border);
    }
}