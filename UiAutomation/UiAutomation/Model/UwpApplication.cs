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

using System;
using System.Diagnostics;

namespace UiAutomation.Model
{
    internal class UwpApplication : IApplication
    {
        private readonly Process _process;

        private Control _control;

        public UwpApplication(Process process) => _process = process;

        public UwpApplication(string identifier, string arguments)
        {
            using (var app = new AppLauncher(identifier))
            {
                if (!app.Exists) _process = null;
                var pid = app.Launch(arguments);
                _process = pid == null ? null : Process.GetProcessById(pid.Value);
            }
        }

        private Process ParentProcess
        {
            get
            {
                var parentProcessId = WindowControl.Parent.CurrentProcessId;
                return Process.GetProcessById(parentProcessId);
            }
        }

        public string ApplicationType => "UWP";

        // Empirically found that it takes about 2 seconds for an UWP process to end. Using a margin.
        public bool Exit(bool force) => Exit(force, 3000);

        public bool Exit(bool force, int timeoutInMilliseconds)
        {
            if (_process == null) return true;
            _process.Refresh();
            if (_process.HasExited) return true;
            return ParentProcess.Exit(force) && _process.WaitForExit(force, timeoutInMilliseconds);
        }

        public bool IsActive => _process != null && !_process.HasExited;
        public int ProcessId => _process.Id;

        public void WaitForInputIdle()
        {
            try
            {
                _process?.WaitForInputIdle();
            }
            catch (InvalidOperationException)
            {
                //ignore, can happen if the application has no UI
            }
        }

        public Control WindowControl
        {
            // don't want to do this multiple times as can be time consuming. So memorizing it after making it.
            get
            {
                if (_process == null || _process.HasExited) return null;
                return _control ?? (_control = Control.CreateControlWithParent("ProcessId:" + _process.Id));
            }
        }
    }
}