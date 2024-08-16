// Copyright 2019-2024 Rik Essenius
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

namespace UiAutomation.Model;

internal class UwpApplication : BaseApplication
{
    public UwpApplication(Process process) : base(process)
    {
    }

    public UwpApplication(string identifier, string arguments)
    {
        using var app = new AppLauncher(identifier);
        if (!app.Exists) return; // process will be set to null via the base
        var pid = app.Launch(arguments);
        _process = pid == null ? null : Process.GetProcessById(pid.Value);
    }

    public override string ApplicationType => "UWP";

    public override Control WindowControl =>
        // UWP application windows can run in subwindows, and not directly under the desktop.
        // So we try to create a 'contained control'.
        // We do this in a loop because sometimes it takes longer for the container to get recognized.
        !IsActive ? null : Control.CreateContainedWindowControl(_process.Id);

    public override bool Exit(bool force)
    {
        if (_process == null) return true;
        _process.Refresh();
        if (_process.HasExited) return true;
        return _process.Exit(force) && _process.WaitForExit(force);
    }
}