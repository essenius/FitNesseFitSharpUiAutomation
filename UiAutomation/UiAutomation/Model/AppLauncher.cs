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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Globalization.CultureInfo;
// ReSharper disable UnusedMember.Global - used for completeness

namespace UiAutomation.Model;

internal enum ActivateOptions
{
    None = 0x00000000,
    DesignMode = 0x00000001, // For for design mode
    NoErrorUi = 0x00000002, // No error dialog if the app fails to activate                                
    NoSplashScreen = 0x00000004 // No splash screen when activating the app
}

[ComImport]
[Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IApplicationActivationManager
{
    // Activates the specified immersive application for the "Launch" contract, passing the provided arguments
    // string into the application. Callers can obtain the process Id of the application instance fulfilling this contract.
    IntPtr ActivateApplication(
        [In] [MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In] [MarshalAs(UnmanagedType.LPWStr)] string arguments,
        [In] ActivateOptions options,
        [Out] out int processId);

    IntPtr ActivateForFile(
        [In] [MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In] IntPtr itemArray,
        [In] [MarshalAs(UnmanagedType.LPWStr)] string verb,
        [Out] out int processId);

    IntPtr ActivateForProtocol(
        [In] [MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In] IntPtr itemArray,
        [Out] out int processId);
}

[ComImport]
[Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
internal class ApplicationActivationManager : IApplicationActivationManager
{
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern IntPtr ActivateApplication(
        [In] string appUserModelId,
        [In] string arguments,
        [In] ActivateOptions options,
        [Out] out int processId);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern IntPtr ActivateForFile(
        [In] string appUserModelId,
        [In] IntPtr itemArray,
        [In] string verb,
        [Out] out int processId);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern IntPtr ActivateForProtocol(
        [In] string appUserModelId,
        [In] IntPtr itemArray,
        [Out] out int processId);
}

internal sealed class AppLauncher : IDisposable
{
    private const int ErrorInsufficientBuffer = 122;
    private const int ErrorNotFound = 87;
    private const int NoError = 0;

    private readonly SafeAppHandle _packageInfo;

    /// <summary>
    ///     Launch an UWB application. You can use the family name as well as the full name.
    ///     if there are more options for full name, it will prefer the version that corresponds to the architecture
    ///     e.g. the X64 version on X64 machines
    /// </summary>
    /// <param name="packageName">Family name or full name of the package</param>
    public AppLauncher(string packageName)
    {
        try
        {
            _packageInfo = null;
            var error = NativeMethods.OpenPackageInfoByFullName(packageName, 0, out _packageInfo);
            if (error != NoError)
            {
                FullName = GetFullName(packageName);
                try
                {
                    error = NativeMethods.OpenPackageInfoByFullName(FullName, 0, out _packageInfo);
                }
                catch (Win32Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
            Debug.Assert(error is NoError or ErrorNotFound, "error is " + error.ToString(InvariantCulture));
        }
        catch (EntryPointNotFoundException)
        {
            // we are not on a platform that supports Apps. _packageInfo is already IntPtr.Zero.
        }
    }

    public bool Exists => _packageInfo.AppExists;

    public string FullName { get; }

    private static bool IsWindows7OrLower
    {
        get
        {
            var versionMajor = Environment.OSVersion.Version.Major;
            var versionMinor = Environment.OSVersion.Version.Minor;
            var version = versionMajor + (double)versionMinor / 10;
            return version <= 6.1;
        }
    }

    public void Dispose() => _packageInfo.Dispose();

    private static string GetFullName(string family)
    {
        var count = 0;
        var bufferLength = 0;
        var error = NativeMethods.GetPackagesByPackageFamily(family, ref count, null, ref bufferLength, null);
        if (error != ErrorInsufficientBuffer) return null;
        var ignore = new IntPtr[count];
        bufferLength++;
        var buffer = new char[bufferLength];
        var error2 = NativeMethods.GetPackagesByPackageFamily(family, ref count, ignore, ref bufferLength, buffer);
        if (error2 != NoError) return null;
        var names = buffer.Unpack();
        if (count == 1) return names[0];
        var architecture = Environment.Is64BitOperatingSystem ? "_x64_" : "_x86_";
        foreach (var name in names)
        {
            if (name.Contains(architecture) || name.Contains("_neutral_")) return name;
        }
        return names[0];
    }

    public static bool IsUwpApp(IntPtr processHandle)
    {
        const long appModelErrorNoPackage = 15700L;

        if (IsWindows7OrLower) return false;

        // we're not interested in the actual family name, just in whether we get an error result 
        // if we wanted to get the name, we needed to run the function again with the right buffer size
        uint length = 0;
        var stringBuilder = new StringBuilder(0);
        var result = NativeMethods.GetPackageFamilyName(processHandle, ref length, stringBuilder);
        return result != appModelErrorNoPackage;
    }

    public int? Launch(string arguments)
    {
        if (!Exists) return null;
        var packageApplicationId = PackageApplicationId();
        var activation = (IApplicationActivationManager)new ApplicationActivationManager();
        var hResult = activation.ActivateApplication(packageApplicationId, arguments ?? string.Empty,
            ActivateOptions.NoErrorUi, out var processId);
        if (hResult != IntPtr.Zero)
        {
            Marshal.ThrowExceptionForHR(hResult.ToInt32());
        }
        return processId;
    }

    private string PackageApplicationId()
    {
        if (!Exists) return null;
        var bufferLength = 0;
        var error = NativeMethods.GetPackageApplicationIds(_packageInfo, ref bufferLength, null, out _);
        Debug.Assert(error == ErrorInsufficientBuffer, "error2 == " + error.ToString(InvariantCulture));
        var buffer = new byte[bufferLength];
        error = NativeMethods.GetPackageApplicationIds(_packageInfo, ref bufferLength, buffer, out var appIdCount);
        Debug.Assert(error == 0, "error3 == " + error.ToString(InvariantCulture));
        return Encoding.Unicode.GetString(buffer, IntPtr.Size * appIdCount,
            bufferLength - IntPtr.Size * appIdCount);
    }
}