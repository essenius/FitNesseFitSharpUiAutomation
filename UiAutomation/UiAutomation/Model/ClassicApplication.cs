// Copyright 2017-2019 Rik Essenius
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
    internal class ClassicApplication : IApplication
    {
        private readonly Process _process;

        public ClassicApplication(Process process) => _process = process;

        public ClassicApplication(string path, string arguments, string workFolder)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = workFolder,
                Arguments = arguments,
                FileName = path,
                UseShellExecute = false
            };
            _process = Process.Start(startInfo);
        }

        public IntPtr MainWindowHandle => _process.MainWindowHandle;

        public string ApplicationType => "Classic";

        public bool Exit(bool force) => Exit(force, 1000);

        public bool Exit(bool force, int timeoutInMilliseconds) =>
            _process.Exit(force) && _process.WaitForExit(force, timeoutInMilliseconds);

        public bool IsActive => _process != null && !_process.HasExited;

        public int ProcessId => _process.Id;

        public void WaitForInputIdle() => _process?.WaitForInputIdle();

        public Control WindowControl =>
            !IsActive ? null : new Control(null, SearchType.Shallow, "ProcessId:" + _process.Id);
    }
}