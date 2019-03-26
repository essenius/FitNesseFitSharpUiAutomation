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
using System.Windows.Forms;

namespace UiAutomation.Model
{
    internal class UwpApplication : BaseApplication
    {
        public UwpApplication(Process process) : base(process)
        {
        }

        public UwpApplication(string identifier, string arguments)
        {
            using (var app = new AppLauncher(identifier))
            {
                if (!app.Exists) return; // process will be set to null via the base
                var pid = app.Launch(arguments);
                process = pid == null ? null : Process.GetProcessById(pid.Value);
            }
        }

        public override string ApplicationType => "UWP";

        private Process ParentProcess
        {
            get
            {
                var parent = WindowControl.Parent;
                var parentProcessId = parent.CurrentProcessId;
                return Process.GetProcessById(parentProcessId);
            }
        }

        public override Control WindowControl
        {
            get
            {
                // UWP application windows run in containers of class ApplicationFrameWindow, and not directly under the desktop.
                // So we try to create a 'contained control'.
                // We do this in a loop because sometimes it takes longer for the container to get recognized.
                const string containerCriterion = "ClassName:ApplicationFrameWindow";

                if (!IsActive) return null;
                Control control = null;
                process.WaitWithTimeoutTill(process1 =>
                    {
                        control = Control.CreateContainedWindowControl(containerCriterion, "ProcessId:" + process1.Id);
                        return control != null;
                    });
                return control;
            }
        }

        public override bool Exit(bool force)
        {
            if (process == null) return true;
            process.Refresh();
            if (process.HasExited) return true;
            return ParentProcess.Exit(force) && process.WaitForExit(force);
        }
    }
}