﻿// Copyright 2017-2021 Rik Essenius
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
    internal abstract class BaseApplication
    {
        protected Process process;

        protected BaseApplication() => process = null;
        protected BaseApplication(Process process) => this.process = process;

        public abstract string ApplicationType { get; }
        public virtual bool IsActive => process is { HasExited: false };
        public virtual IntPtr MainWindowHandle => process.MainWindowHandle;
        public virtual int ProcessId => process.Id;
        public abstract Control WindowControl { get; }
        public abstract bool Exit(bool force);

        public virtual void WaitForInputIdle()
        {
            try
            {
                if (process == null) return;
                process.WaitForInputIdle();
            }
            catch (InvalidOperationException)
            {
                //ignore, can happen if the application has no UI
            }
        }
    }
}
