using System;
using System.Runtime.InteropServices;

namespace Ecorise.Utils
{
    public class PreventSleepMode : IDisposable
    {
        private ExecutionState previousExecutionState;

        public PreventSleepMode()
        {
            // Prevent sleep mode. See http://stackoverflow.com/questions/6302185/how-to-prevent-windows-from-entering-idle-state
            previousExecutionState = NativeMethods.SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Restore previous state
                NativeMethods.SetThreadExecutionState(previousExecutionState);

                disposedValue = true;
            }
        }

        ~PreventSleepMode()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    [Flags]
    public enum ExecutionState : UInt32
    {
        Continuous = 0x80000000,
        AwayModeRequired = 0x00000040,
        SystemRequired = 0x00000001,
        DisplayRequired = 0x00000002,
        UserPresent = 0x00000004
    }

    public static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern ExecutionState SetThreadExecutionState(ExecutionState executionState);
    }

}
