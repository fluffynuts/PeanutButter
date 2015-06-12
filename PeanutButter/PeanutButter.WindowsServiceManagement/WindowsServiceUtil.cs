using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace PeanutButter.Win32ServiceControl
{
    public interface IWindowsServiceUtil
    {
        string ServiceExe { get; set; }
        ServiceState State { get; }
        bool IsInstalled { get; }
        int ServiceStateExtraWaitSeconds { get; set; }
        ServiceStartupTypes StartupType { get; }

        void Uninstall();
        void InstallAndStart();
        void Install();
        void Start(bool wait = true);
        void Stop(bool wait = true);
        void Pause();
        void Continue();
        void Disable();
        void SetAutomaticStart();
        void SetManualStart();
    }

    public enum ServiceStartupTypes
    {
        Unknown = -1,
        Boot = 0,
        System = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4
    }

    public class WindowsServiceUtilException : Exception
    {
        public WindowsServiceUtilException(string message)
            : base (message)
        {
        }
    }

    public class ServiceNotInstalledException : WindowsServiceUtilException
    {
        public ServiceNotInstalledException(string serviceName)
            : base(serviceName + " is not installed")
        {
        }
    }

    public class ServiceOperationException : WindowsServiceUtilException
    {
        public ServiceOperationException(string serviceName, string operation, string info)
            : base("Unable to perform " + (operation ?? "(null)") + " on service " + (serviceName ?? "(null)") + ": " + (info ?? "(null)"))
        {
        }
    }

    public class WindowsServiceUtil : IWindowsServiceUtil
    {
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const string _serviceNotInstalled = "Service not installed";
        private const int ERROR_SERVICE_MARKED_FOR_DELETE = 1072;
        private enum SC_STATUS_TYPE
        {
            SC_STATUS_PROCESS_INFO = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        private class SERVICE_STATUS
        {
            public int dwServiceType = 0;
            public ServiceState dwCurrentState = 0;
            public int dwControlsAccepted = 0;
            public int dwWin32ExitCode = 0;
            public int dwServiceSpecificExitCode = 0;
            public int dwCheckPoint = 0;
            public int dwWaitHint = 0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class SERVICE_STATUS_PROCESS
        {
            public int dwServiceType = 0;
            public ServiceState dwCurrentState = 0;
            public int dwControlsAccepted = 0;
            public int dwWin32ExitCode = 0;
            public int dwServiceSpecificExitCode = 0;
            public int dwCheckPoint = 0;
            public int dwWaitHint = 0;
            public int dwProcessID = 0;
            public int dwServiceFlags = 0;
        }

        #region dllImports
        #region OpenSCManager

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern IntPtr OpenSCManager(string machineName, string databaseName,
                                                   ScmAccessRights dwDesiredAccess);

        #endregion

        #region OpenService

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName,
                                                 ServiceAccessRights dwDesiredAccess);

        #endregion

        #region CreateService

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName,
                                                   ServiceAccessRights dwDesiredAccess, int dwServiceType,
                                                   ServiceBootFlag dwStartType, ServiceError dwErrorControl,
                                                   string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId,
                                                   string lpDependencies, string lp, string lpPassword);

        #endregion

        #region CloseServiceHandle

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        #endregion

        #region QueryServiceStatus

        [DllImport("advapi32.dll")]
        private static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);
        [DllImport("advapi32.dll")]
        private static extern bool QueryServiceStatusEx(IntPtr serviceHandle, int infoLevel, IntPtr buffer, int bufferSize, out int bytesNeeded);

        #endregion

        #region DeleteService

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteService(IntPtr hService);

        #endregion

        #region ControlService

        [DllImport("advapi32.dll")]
        private static extern int ControlService(IntPtr hService, ServiceControl dwControl,
                                                 SERVICE_STATUS lpServiceStatus);

        #endregion

        #region StartService

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);

        #endregion

        #region ChangeServiceConfig2
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
        
        #endregion

        #endregion dllImports

        private string _serviceName;
        public string ServiceName { get { return this._serviceName; } }
        public string DisplayName 
        { 
            get
            {
                return this._displayName;
            }
            set
            {
                if (value == null)
                    this._displayName = this._serviceName;
                else
                    this._displayName = value;
            }
        }
        private string _displayName;
        private string _serviceExe;

        public string ServiceExe
        {
            get
            {
                if (this._serviceExe != null)
                    return this._serviceExe;

                var searcher = new ManagementObjectSearcher(String.Format("select PathName from Win32_Service where Name = '{0}'",
                                                                           this._serviceName.Replace("'", "''")));
                var collection = searcher.Get();
                if (collection.Count == 0)
                    return null;
                foreach (var svc in collection)
                {
                    this._serviceExe = svc["PathName"].ToString();
                    break;
                }
                return this._serviceExe;
            }
            set 
            {
                this._serviceExe = value;
            }
        }

        public static WindowsServiceUtil GetServiceByPath(string path)
        {
            var searcher = new ManagementObjectSearcher(String.Join(String.Empty,
                new string[] { "select Name from Win32_Service where PathName = '", path.Replace("'", "''").Replace("\\", "\\\\"), "'" }));
            var collection = searcher.Get();
            if (collection.Count == 0)
                return null;
            string svcName = null;
            foreach (var item in collection)
            {
                svcName = item.Properties["Name"].Value.ToString();
                break;
            }
            if (svcName == null)
                return null;
            return new WindowsServiceUtil(svcName);
        }

        public int ServiceStateExtraWaitSeconds { get; set; }

        public ServiceState State
        {
            get
            {
                var ret = ServiceState.Unknown;

                IntPtr scm = this.OpenSCManager(ScmAccessRights.Connect);

                try
                {
                    IntPtr service = WindowsServiceUtil.OpenService(scm, this._serviceName, ServiceAccessRights.QueryStatus);
                    if (service == IntPtr.Zero)
                        ret = ServiceState.NotFound;
                    else
                    {
                        try
                        {
                            ret = this.GetServiceStatus(service);
                        }
                        finally
                        {
                            CloseServiceHandle(service);
                        }
                    }
                }
                finally
                {
                    CloseServiceHandle(scm);
                }
                return ret;
            }
        }

        public bool IsInstalled
        {
            get
            {
                IntPtr scm = this.OpenSCManager(ScmAccessRights.Connect);
                var ret = false;
                try
                {
                    IntPtr service = OpenService(scm, this._serviceName, ServiceAccessRights.QueryStatus);
                    ret = (service != IntPtr.Zero);
                    CloseServiceHandle(service);
                }
                finally
                {
                    CloseServiceHandle(scm);
                }
                return ret;
            }
        }

        private uint _serviceStopTimeoutMS;
        public uint ServiceStopTimeoutMS
        {
            get { return this._serviceStopTimeoutMS; }
            set { this._serviceStopTimeoutMS = value; }
        }

        public WindowsServiceUtil(string serviceName, string displayName = null, string defaultServiceExePath = null)
        {
            this._serviceName = serviceName;
            this._displayName = displayName;
            if (this._displayName == null)
                this._displayName = this._serviceName;
            if (this.ServiceExe == null)    // see if we can get an existing path else set to the defaultServicePath for installations
                this.ServiceExe = defaultServiceExePath;
        }

        public void Uninstall()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = OpenService(scm, this._serviceName, ServiceAccessRights.AllAccess);
                if (service == IntPtr.Zero)
                    throw new ServiceNotInstalledException(this._serviceName);

                try
                {
                    StopService(service);
                    if (!DeleteService(service))
                    {
                        var err = Marshal.GetLastWin32Error();
                        if (err != ERROR_SERVICE_MARKED_FOR_DELETE)
                        {
                            var lastError = new Win32Exception(err);
                            throw new ServiceOperationException(this._serviceName, "Delete", lastError.ErrorCode.ToString() + ": " + lastError.Message);
                        }
                    }
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public int ServicePID
        {
            get
            {
                IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
                IntPtr service = OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                IntPtr buf = IntPtr.Zero;
                try
                {
                int size = 0;
                QueryServiceStatusEx(service, 0, buf, size, out size);
                buf = Marshal.AllocHGlobal(size);
                if (!QueryServiceStatusEx(service, 0, buf, size, out size))
                {
                    return -1;
                }
                var status = (SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(buf, typeof(SERVICE_STATUS_PROCESS));
                return status.dwProcessID;
                }
                finally
                {
                    if (!buf.Equals(IntPtr.Zero))
                        Marshal.FreeHGlobal(buf);
                }
            }
        }

        public void InstallAndStart()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = OpenService(scm, this._serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    service = this.DoServiceInstall(scm);

                try
                {
                    this.StartService(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public void Install()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                CloseServiceHandle(DoServiceInstall(scm));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        private IntPtr DoServiceInstall(IntPtr scm)
        {
            if (this.ServiceExe == null)
                throw new ServiceOperationException(this._serviceName, "Install", "no ServiceExe set");

            if (!File.Exists(this.ServiceExe))
                throw new ServiceOperationException(this._serviceName, "Install", "Can't find service executable at: " + this._serviceExe);

            var service = CreateService(scm, this._serviceName, this._displayName, ServiceAccessRights.AllAccess,
                                    SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.AutoStart, ServiceError.Normal,
                                    this._serviceExe, null, IntPtr.Zero, null, null, null);
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            if (service == IntPtr.Zero)
                throw new ServiceOperationException(this._serviceName, "Install", 
                    String.Join(String.Empty, new string[] { "Failed to install with executable: " + this._serviceExe, "\nMore info:\n", win32Exception.Message }));

            return service;
        }

        public ServiceStartupTypes StartupType 
        {
            get 
            { 
                var regKey = GetServiceRegistryKey(); 
                if (regKey == null) return ServiceStartupTypes.Unknown;
                ServiceStartupTypes result;
                try
                {
                    return
                        (ServiceStartupTypes)
                            Enum.Parse(typeof (ServiceStartupTypes), regKey.GetValue("Start").ToString(), true);
                }
                catch (Exception)
                {
                    return ServiceStartupTypes.Unknown;
                }
            }
        }

        public void Disable()
        {
            SetStartValue(ServiceStartupTypes.Disabled);
        }

        public void SetAutomaticStart()
        {
            SetStartValue(ServiceStartupTypes.Automatic);
        }

        public void SetManualStart()
        {
            SetStartValue(ServiceStartupTypes.Manual);
        }


        private void SetStartValue(ServiceStartupTypes value)
        {
            if (!IsInstalled) return;
            var regKey = GetServiceRegistryKey();
            if (regKey == null) throw new Exception("Unable to open service registry key for '" + ServiceName + "'");
            regKey.SetValue("Start", value, RegistryValueKind.DWord);
        }

        private RegistryKey GetServiceRegistryKey()
        {
            try
            {
                var subKeypath = String.Join("\\", new[] { 
                    "SYSTEM", "CurrentControlSet", "Services", ServiceName 
                });
                return Registry.LocalMachine.OpenSubKey(subKeypath, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Start(bool wait = true)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(this._serviceName, "Start", _serviceNotInstalled);
                try
                {
                    this.StartService(service, wait);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public void Stop(bool wait = true)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(this._serviceName, "Stop", _serviceNotInstalled);

                try
                {
                    this.StopService(service, wait);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public void Pause()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(this._serviceName, "Pause", _serviceNotInstalled);
                try
                {
                    this.PauseService(service);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        public void Continue()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(this._serviceName, "Pause", _serviceNotInstalled);
                try
                {
                    this.PauseService(service);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        private void StartService(IntPtr service, bool wait = true)
        {
            if (this.GetServiceStatus(service) == ServiceState.Running)
                return;
            StartService(service, 0, 0);
            if (wait)
            {
                var changedStatus = this.WaitForServiceStatus(service, ServiceState.StartPending, ServiceState.Running);
                if (!changedStatus)
                    throw new ServiceOperationException(this._serviceName, "Start", "Unable to start service");
            }
        }

        private void StopService(IntPtr service, bool wait = true)
        {
            if (this.GetServiceStatus(service) != ServiceState.Running)
                return;
            var process = Process.GetProcessById(this.ServicePID);
            SERVICE_STATUS status = new SERVICE_STATUS();
            ControlService(service, ServiceControl.Stop, status);
            const string op = "Stop";
            if (wait)
            {
                var changedStatus = this.WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
                if (!changedStatus)
                    throw new ServiceOperationException(this._serviceName, op, "Unable to stop service");
                var waitLevel = 0;
                try
                {
                    if (!process.HasExited)
                    {
                        waitLevel++;
                        if (!process.WaitForExit((int)this._serviceStopTimeoutMS))
                        {
                            waitLevel++;
                            process.Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    switch (waitLevel)
                    {
                        case 0:
                            throw new ServiceOperationException(this._serviceName, op, 
                                GenerateStopExceptionMessageWith("but threw errors when interrogating process state", ex));
                        case 1:
                            throw new ServiceOperationException(this._serviceName, op, 
                                GenerateStopExceptionMessageWith("and we got an error whilst waiting 10 seconds for graceful exit", ex));
                        case 2:
                            throw new ServiceOperationException(this._serviceName, op, 
                                GenerateStopExceptionMessageWith("and we got an error after trying to kill it when it didn't gracefully exit within 10 seconds", ex));

                    }
                }
            }
        }

        private string GenerateStopExceptionMessageWith(string subMessage, Exception ex)
        {
            return String.Format("{0} {1} ({2})",
                "Service responded to stop command",
                subMessage,
                ex.Message);
        }

        private void PauseService(IntPtr service, bool wait = true)
        {
            if (this.GetServiceStatus(service) != ServiceState.Running)
                throw new ServiceOperationException(this._serviceName, "Pause", "Cannot pause a service which isn't already running");
            var status = new SERVICE_STATUS();
            ControlService(service, ServiceControl.Pause, status);
            if (wait)
            {
                var changedStatus = this.WaitForServiceStatus(service, ServiceState.PausePending, ServiceState.Paused);
                if (!changedStatus)
                    throw new ServiceOperationException(this._serviceName, "Pause", "Unable to pause service");
            }
        }

        private void ContinueService(IntPtr service, bool wait = true)
        {
            if (this.GetServiceStatus(service) != ServiceState.Paused)
                throw new ServiceOperationException(this._serviceName, "Continue", "Cannot continue a service not in the paused state");
            var status = new SERVICE_STATUS();
            ControlService(service, ServiceControl.Continue, status);
            if (wait)
            {
                var changedStatus = this.WaitForServiceStatus(service, ServiceState.ContinuePending, ServiceState.Running);
                if (!changedStatus)
                    throw new ServiceOperationException(this._serviceName, "Continue", "Unable to continue service");
            }
        }

        private ServiceState GetServiceStatus(IntPtr service)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();

            if (QueryServiceStatus(service, status) == 0)
                throw new ServiceOperationException(this._serviceName, "GetServiceStatus", "Failed to query service status");
            return status.dwCurrentState;
        }

        private bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();

            QueryServiceStatus(service, status);
            if (status.dwCurrentState == desiredStatus) return true;

            int dwStartTickCount = Environment.TickCount;
            int dwOldCheckPoint = status.dwCheckPoint;

            while (status.dwCurrentState == waitStatus)
            {
                // Do not wait longer than the wait hint. A good interval is
                // one tenth the wait hint, but no less than 1 second and no
                // more than 10 seconds.

                int dwWaitTime = status.dwWaitHint/10;

                if (dwWaitTime < 1000) dwWaitTime = 1000;
                else if (dwWaitTime > 10000) dwWaitTime = 10000;

                Thread.Sleep(dwWaitTime);

                // Check the status again.

                if (QueryServiceStatus(service, status) == 0) break;

                if (status.dwCheckPoint > dwOldCheckPoint)
                {
                    // The service is making progress.
                    dwStartTickCount = Environment.TickCount;
                    dwOldCheckPoint = status.dwCheckPoint;
                }
                else
                {
                    if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
                    {
                        // No progress made within the wait hint
                        break;
                    }
                }
            }
            if ((status.dwCurrentState != desiredStatus) && (this.ServiceStateExtraWaitSeconds > 0))
            {
                var waited = 0;
                while ((waited < this.ServiceStateExtraWaitSeconds) && (status.dwCurrentState != desiredStatus))
                {
                    Thread.Sleep(1000);
                    waited++;
                    if (QueryServiceStatus(service, status) == 0) break;
                }
            }
            if (status.dwCurrentState != desiredStatus)
                return KillService();
            return (status.dwCurrentState == desiredStatus);
        }

        public bool KillService()
        {
            var killThese = new List<Process>();
            var myProc = Process.GetCurrentProcess();
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.Id == myProc.Id)
                    continue;
                try
                {
                    if (proc.MainModule.FileName.ToLower() == this.ServiceExe)
                    {
                        try
                        {
                            proc.Kill();
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    // happens if a 32-bit process tries to read a 64-bit process' info
                    if (proc.ProcessName.ToLower() == Path.GetFileName(this.ServiceExe).ToLower())
                        killThese.Add(proc);
                }
            }
            if (killThese.Count == 0)
                return true;    // not running any more
            int killed = 0;
            foreach (var proc in killThese)
            {
                try
                {
                    if (!proc.HasExited)
                        proc.Kill();
                    killed++;
                }
                catch (Exception) { }
            }
            return (killed == killThese.Count);
        }

        private IntPtr OpenSCManager(ScmAccessRights rights)
        {
            IntPtr scm = OpenSCManager(null, null, rights);
            if (scm == IntPtr.Zero)
                throw new ServiceOperationException(this._serviceName, "OpenSCManager", "Could not connect to service control manager");

            return scm;
        }

    }


    public enum ServiceState
    {
        Unknown = -1, // The state cannot be (has not been) retrieved.
        NotFound = 0, // The service is not known on the host server.
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }

    [Flags]
    public enum ScmAccessRights
    {
        Connect = 0x0001,
        CreateService = 0x0002,
        EnumerateService = 0x0004,
        Lock = 0x0008,
        QueryLockStatus = 0x0010,
        ModifyBootConfig = 0x0020,
        StandardRightsRequired = 0xF0000,

        AllAccess = (StandardRightsRequired | Connect | CreateService |
                     EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
    }

    [Flags]
    public enum ServiceAccessRights
    {
        QueryConfig = 0x1,
        ChangeConfig = 0x2,
        QueryStatus = 0x4,
        EnumerateDependants = 0x8,
        Start = 0x10,
        Stop = 0x20,
        PauseContinue = 0x40,
        Interrogate = 0x80,
        UserDefinedControl = 0x100,
        Delete = 0x00010000,
        StandardRightsRequired = 0xF0000,

        AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig |
                     QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
                     Interrogate | UserDefinedControl)
    }

    public enum ServiceBootFlag
    {
        Start = 0x00000000,
        SystemStart = 0x00000001,
        AutoStart = 0x00000002,
        DemandStart = 0x00000003,
        Disabled = 0x00000004
    }

    public enum ServiceControl
    {
        Stop = 0x00000001,
        Pause = 0x00000002,
        Continue = 0x00000003,
        Interrogate = 0x00000004,
        Shutdown = 0x00000005,
        ParamChange = 0x00000006,
        NetBindAdd = 0x00000007,
        NetBindRemove = 0x00000008,
        NetBindEnable = 0x00000009,
        NetBindDisable = 0x0000000A
    }

    public enum ServiceError
    {
        Ignore = 0x00000000,
        Normal = 0x00000001,
        Severe = 0x00000002,
        Critical = 0x00000003
    }}
