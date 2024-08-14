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

using System.ComponentModel;

namespace UiAutomation.Model
{
    internal static class ApplicationFactory
    {
        public static BaseApplication AttachToProcess(int processId)
        {
            var process = new ProcessHandler(processId).ProcessObject();
            if (AppLauncher.IsUwpApp(process.Handle)) return new UwpApplication(process);
            return new ClassicApplication(process);
        }

        public static BaseApplication Start(string identifier, string arguments)
        {
            const int fileNotFound = 0x02;
            const int accessDenied = 0x05;
            try
            {
                return new ClassicApplication(identifier, arguments, null);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == fileNotFound || ex.NativeErrorCode == accessDenied)
            {
                var app = new UwpApplication(identifier, arguments);
                return app.IsActive ? app : null;
            }
        }

        public static BaseApplication Start(string identifier, string arguments, string workFolder) =>
            string.IsNullOrEmpty(workFolder)
                ? Start(identifier, arguments)
                : new ClassicApplication(identifier, arguments, workFolder);
    }
}
