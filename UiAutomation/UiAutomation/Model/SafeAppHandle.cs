﻿// Copyright 2019-2024 Rik Essenius
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
using Microsoft.Win32.SafeHandles;

namespace UiAutomation.Model;

internal class SafeAppHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private const long Success = 0;

    private SafeAppHandle() : base(true)
    {
    }

    public bool AppExists => handle != IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (handle == IntPtr.Zero) return true;
        return NativeMethods.ClosePackageInfo(handle) == Success;
    }
}