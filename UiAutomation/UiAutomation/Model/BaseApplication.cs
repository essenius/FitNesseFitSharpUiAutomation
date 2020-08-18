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
        public virtual bool IsActive => process != null && !process.HasExited;
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
