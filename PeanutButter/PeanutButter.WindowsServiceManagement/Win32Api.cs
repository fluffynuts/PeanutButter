using System;
using System.Runtime.InteropServices;

namespace PeanutButter.Win32ServiceControl
{
    public class Win32Api
    {
        public const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public const int ERROR_SERVICE_MARKED_FOR_DELETE = 1072;

        [StructLayout(LayoutKind.Sequential)]
        public class SERVICE_STATUS
        {
            public int ServiceType = 0;
            public ServiceState CurrentState = 0;
            public int ControlsAccepted = 0;
            public int Win32ExitCode = 0;
            public int ServiceSpecificExitCode = 0;
            public int CheckPoint = 0;
            public int WaitHint = 0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class SERVICE_STATUS_PROCESS
        {
            public int ServiceType = 0;
            public ServiceState CurrentState = 0;
            public int ControlsAccepted = 0;
            public int Win32ExitCode = 0;
            public int ServiceSpecificExitCode = 0;
            public int CheckPoint = 0;
            public int WaitHint = 0;
            public int ProcessID = 0;
            public int ServiceFlags = 0;
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
            SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName,
            ScmAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName,
            ServiceAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName,
            ServiceAccessRights dwDesiredAccess, int dwServiceType,
            ServiceBootFlag dwStartType, ServiceError dwErrorControl,
            string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId,
            string lpDependencies, string lp, string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll")]
        public static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll")]
        public static extern bool QueryServiceStatusEx(IntPtr serviceHandle, int infoLevel, IntPtr buffer, int bufferSize, out int bytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll")]
        public static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);

        public enum ServiceConfigTypes
        {
            Description = 1,
            FailureActions = 2,
            DelayedAutoStartInfo = 3,
            FailureActionsFlag = 4,
            ServiceSidInfo = 5,
            RequiredPrivilegesInfo = 6,
            PreshutdownInfo = 7
        }

        [DllImport("advapi32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2(IntPtr hService,int dwInfoLevel,IntPtr lpInfo);
    }
}
