using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
[assembly: InternalsVisibleTo("PeanutButter.WindowsServiceManagement.Tests")]

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace PeanutButter.WindowsServiceManagement
{
    internal class Win32Api
    {
        public const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public const int ERROR_SERVICE_MARKED_FOR_DELETE = 1072;

        [StructLayout(LayoutKind.Sequential)]
        public class SERVICE_STATUS
        {
            public int ServiceType;
            public ServiceState CurrentState = 0;
            public int ControlsAccepted;
            public int Win32ExitCode;
            public int ServiceSpecificExitCode;
            public int CheckPoint;
            public int WaitHint;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class SERVICE_STATUS_PROCESS
        {
            public int ServiceType;
            public ServiceState CurrentState = 0;
            public int ControlsAccepted;
            public int Win32ExitCode;
            public int ServiceSpecificExitCode;
            public int CheckPoint;
            public int WaitHint;
            public int ProcessID;
            public int ServiceFlags;
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
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SERVICE_DELAYED_AUTO_START_INFO
        {
            public bool fDelayedAutostart;
        }
        public const int SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;
        
    }
}
