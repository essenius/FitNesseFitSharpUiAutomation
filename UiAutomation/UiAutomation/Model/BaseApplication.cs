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

using System;
using System.Diagnostics;
using System.Threading;

namespace UiAutomation.Model;

internal abstract class BaseApplication(Process process)
{
    protected Process _process = process;

    protected BaseApplication() : this(null)
    {
    }

    public abstract string ApplicationType { get; }

    public virtual bool IsActive => _process is { HasExited: false };
    public virtual IntPtr MainWindowHandle => _process.MainWindowHandle;
    public virtual int ProcessId => _process.Id;
    public abstract Control WindowControl { get; }
    public abstract bool Exit(bool force);

    public virtual void WaitForInputIdle()
    {
        try
        {
            if (_process == null) return;
            _process.WaitForInputIdle();
        }
        catch (InvalidOperationException)
        {
            //ignore, can happen if the application has no UI
        }
    }

    public virtual bool WaitForMainWindow()
    {
        var tries = 0;
        while (MainWindowHandle == IntPtr.Zero && tries < 20)
        {
            Thread.Sleep(100);
            tries++;
        }

        return MainWindowHandle != IntPtr.Zero;
    }
}