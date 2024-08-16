// Copyright 2017-2024 Rik Essenius
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
using static System.Globalization.CultureInfo;

namespace UiAutomation.Model;

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

        _process = Process.Start(startInfo);
    }

    public override string ApplicationType => "Classic";

    public override Control WindowControl =>
        !IsActive
            ? null
            : new Control("ProcessId:" + _process.Id.ToString(InvariantCulture))
            {
                SearchType = SearchType.Shallow,
                Parent = null
            };

    public override bool Exit(bool force) => _process.Exit(force) && _process.WaitForExit(force);
}