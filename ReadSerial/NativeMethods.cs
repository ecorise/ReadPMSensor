namespace Ecorise.Utils
{
    public static class NativeMethods
    {
        // Import SetThreadExecutionState Win32 API and necessary flags
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError=true)]
        internal static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
    }
}
