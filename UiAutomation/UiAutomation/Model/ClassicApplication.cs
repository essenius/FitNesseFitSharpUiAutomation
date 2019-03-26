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

using System.Diagnostics;

namespace UiAutomation.Model
{
    internal class ClassicApplication : BaseApplication
    {
        public ClassicApplication(Process process) : base(process)
        {
        }

        public ClassicApplication(string path, string arguments, string workFolder)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = workFolder,
                Arguments = arguments,
                FileName = path,
                UseShellExecute = false
            };

            process = Process.Start(startInfo);
        }

        public override string ApplicationType => "Classic";

        public override Control WindowControl =>
            !IsActive ? null : new Control(null, SearchType.Shallow, "ProcessId:" + process.Id);

        public override bool Exit(bool force) => process.Exit(force) && process.WaitForExit(force);
    }
}