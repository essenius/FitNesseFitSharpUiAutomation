using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace UiAutomation
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SECURITY_ATTRIBUTES
    {
        public uint nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    internal enum SECURITY_IMPERSONATION_LEVEL
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    internal enum TOKEN_TYPE
    {
        TokenPrimary = 1,
        TokenImpersonation
    }

    public class ProcessAsUser
    {
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const int GENERIC_ALL_ACCESS = 0x10000000;
        private const int STARTF_FORCEONFEEDBACK = 0x00000040;
        private const int STARTF_RUNFULLSCREEN = 0x00000020;
        private const int STARTF_USESHOWWINDOW = 0x00000001;

        private const short SW_SHOW = 5;
        private const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint TOKEN_QUERY = 0x0008;


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr hObject);


        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(
            ref IntPtr lpEnvironment,
            IntPtr hToken,
            bool bInherit);


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);


        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool DestroyEnvironmentBlock(
            IntPtr lpEnvironment);


        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", SetLastError = true)]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            int ImpersonationLevel,
            int dwTokenType,
            ref IntPtr phNewToken);


        private static IntPtr GetEnvironmentBlock(IntPtr token)
        {
            var envBlock = IntPtr.Zero;
            var retVal = CreateEnvironmentBlock(ref envBlock, token, false);
            if (retVal == false)
            {
                //Environment Block, things like common paths to My Documents etc.
                //Will not be created if "false"
                //It should not adversley affect CreateProcessAsUser.

                var message = $"CreateEnvironmentBlock Error: {Marshal.GetLastWin32Error()}";
                Debug.WriteLine(message);
            }
            return envBlock;
        }


        private static IntPtr GetPrimaryToken(int processId)
        {
            var token = IntPtr.Zero;
            var primaryToken = IntPtr.Zero;
            var retVal = false;
            Process p = null;

            try
            {
                p = Process.GetProcessById(processId);
            }

            catch (ArgumentException)
            {
                var details = $"ProcessID {processId} Not Available";
                Debug.WriteLine(details);
                throw;
            }

            //Gets impersonation token
            retVal = OpenProcessToken(p.Handle, TOKEN_DUPLICATE, ref token);
            if (retVal)
            {
                var sa = new SECURITY_ATTRIBUTES();
                sa.nLength = (uint) Marshal.SizeOf(sa);

                //Convert the impersonation token into Primary token
                retVal = DuplicateTokenEx(
                    token,
                    TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_QUERY,
                    ref sa,
                    (int) SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                    (int) TOKEN_TYPE.TokenPrimary,
                    ref primaryToken);

                //Close the Token that was previously opened.
                CloseHandle(token);
                if (!retVal)
                {
                    var message = $"DuplicateTokenEx Error: {Marshal.GetLastWin32Error()}";
                    Debug.WriteLine(message);
                }
            }

            else
            {
                var message = $"OpenProcessToken Error: {Marshal.GetLastWin32Error()}";
                Debug.WriteLine(message);
            }

            //We'll close this token after it is used.
            return primaryToken;
        }


        public static bool Launch(string appCmdLine /*,int processId*/)
        {
            var ret = false;

            //Either specify the processID explicitly
            //Or try to get it from a process owned by the user.
            //In this case assuming there is only one explorer.exe

            var ps = Process.GetProcessesByName("explorer");
            var processId = -1; //=processId
            if (ps.Length > 0)
            {
                processId = ps[0].Id;
            }

            if (processId > 1)
            {
                var token = GetPrimaryToken(processId);

                if (token != IntPtr.Zero)
                {
                    var envBlock = GetEnvironmentBlock(token);
                    ret = LaunchProcessAsUser(appCmdLine, token, envBlock);
                    if (envBlock != IntPtr.Zero)
                        DestroyEnvironmentBlock(envBlock);

                    CloseHandle(token);
                }
            }
            return ret;
        }


        private static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock)
        {
            var result = false;

            var pi = new PROCESS_INFORMATION();
            var saProcess = new SECURITY_ATTRIBUTES();
            var saThread = new SECURITY_ATTRIBUTES();
            saProcess.nLength = (uint) Marshal.SizeOf(saProcess);
            saThread.nLength = (uint) Marshal.SizeOf(saThread);

            var si = new STARTUPINFO();
            si.cb = (uint) Marshal.SizeOf(si);

            //if this member is NULL, the new process inherits the desktop
            //and window station of its parent process. If this member is
            //an empty string, the process does not inherit the desktop and
            //window station of its parent process; instead, the system
            //determines if a new desktop and window station need to be created.
            //If the impersonated user already has a desktop, the system uses the
            //existing desktop.

            si.lpDesktop = @"WinSta0\Default"; //Modify as needed
            si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;
            si.wShowWindow = SW_SHOW;

            //Set other si properties as required.

            result = CreateProcessAsUser(
                token,
                null,
                cmdLine,
                ref saProcess,
                ref saThread,
                false,
                CREATE_UNICODE_ENVIRONMENT,
                envBlock,
                null,
                ref si,
                out pi);

            if (!result)
            {
                var error = Marshal.GetLastWin32Error();
                var message = $"CreateProcessAsUser Error: {error}";
                Debug.WriteLine(message);
            }

            return result;
        }


        /// <summary>
        /// LaunchProcess As User Overloaded for Window Mode 
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <param name="token"></param>
        /// <param name="envBlock"></param>
        /// <param name="WindowMode"></param>
        /// <returns></returns>
        private static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock, uint WindowMode)
        {
            var result = false;

            var pi = new PROCESS_INFORMATION();
            var saProcess = new SECURITY_ATTRIBUTES();
            var saThread = new SECURITY_ATTRIBUTES();
            saProcess.nLength = (uint) Marshal.SizeOf(saProcess);
            saThread.nLength = (uint) Marshal.SizeOf(saThread);

            var si = new STARTUPINFO();
            si.cb = (uint) Marshal.SizeOf(si);

            //if this member is NULL, the new process inherits the desktop
            //and window station of its parent process. If this member is
            //an empty string, the process does not inherit the desktop and
            //window station of its parent process; instead, the system
            //determines if a new desktop and window station need to be created.
            //If the impersonated user already has a desktop, the system uses the
            //existing desktop.

            si.lpDesktop = @"WinSta0\Default"; //Default Vista/7 Desktop Session
            si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;

            //Check the Startup Mode of the Process 
            if (WindowMode == 1)
                si.wShowWindow = SW_SHOW;
            else if (WindowMode == 2)
            {
                //Do Nothing
            }
            else if (WindowMode == 3)
                si.wShowWindow = 0; //Hide Window 
            else if (WindowMode == 4)
                si.wShowWindow = 3; //Maximize Window
            else if (WindowMode == 5)
                si.wShowWindow = 6; //Minimize Window
            else
                si.wShowWindow = SW_SHOW;


            //Set other si properties as required.
            result = CreateProcessAsUser(
                token,
                null,
                cmdLine,
                ref saProcess,
                ref saThread,
                false,
                CREATE_UNICODE_ENVIRONMENT,
                envBlock,
                null,
                ref si,
                out pi);

            if (result == false)
            {
                var error = Marshal.GetLastWin32Error();
                var message = $"CreateProcessAsUser Error: {error}";
                Debug.WriteLine(message);
            }

            return result;
        }


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            ref IntPtr TokenHandle);
    }
}