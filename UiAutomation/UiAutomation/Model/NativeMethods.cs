// Copyright 2013-2019 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace UiAutomation.Model
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Want to be reasonably complete"),
     SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Legacy Windows naming followed")]
    internal static class NativeMethods
    {
        public delegate bool WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        public enum WMessages
        {
            WM_LBUTTONDOWN = 0x201,
            WM_LBUTTONUP = 0x202,
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        // these are App related APIs which do not exist in Win7 and before
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), DllImport("kernel32")]
        public static extern int ClosePackageInfo(IntPtr pir);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), DllImport("kernel32")]
        public static extern int GetPackageApplicationIds(IntPtr pir, ref int bufferLength, byte[] buffer, out int count);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetPackageFamilyName(IntPtr hProcess, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern int GetPackagesByPackageFamily(
            [MarshalAs(UnmanagedType.LPWStr)] string packageFamilyName, ref int count,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)]
            StringBuilder[] packageFullName,
            out int bufferLength, char[] buffer);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int smIndex);

        // we'll deal with it by catching exceptions. See AppLauncher
        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), DllImport("kernel32")]
        public static extern int OpenPackageInfoByFullName([MarshalAs(UnmanagedType.LPWStr)] string fullName, uint reserved,
            out IntPtr packageInfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential)]
        internal struct HardwareInput
        {
            internal int uMsg;
            internal short wParamL;
            internal short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            internal SendInputEventType type;
            internal InputUnion U;
            internal static int Size => Marshal.SizeOf(typeof(Input));
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)] internal MouseInput mi;
            [FieldOffset(0)] internal KeybdInput ki;
            [FieldOffset(0)] internal HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeybdInput
        {
            internal ushort wVk;
            internal ushort wScan;
            internal uint dwFlags;
            internal uint time;
            internal IntPtr dwExtraInfo;
        }

        [Flags]
        internal enum MouseEventFlags : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            HWheel = 0x1000,
            MoveNoCoalesce = 0x2000,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseInput
        {
            internal int x;
            internal int y;
            internal uint mouseData;
            internal uint flags;
            internal uint time;
            internal IntPtr extraInfo;
        }

        internal enum SendInputEventType : uint
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }
    }
}