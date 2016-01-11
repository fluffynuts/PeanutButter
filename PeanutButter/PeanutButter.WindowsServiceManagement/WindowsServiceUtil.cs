using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using PeanutButter.Win32ServiceControl.Exceptions;

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

    public class WindowsServiceUtil : IWindowsServiceUtil
    {
        private const string _serviceNotInstalled = "Service not installed";

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
                ServiceState ret;
                IntPtr scm = this.OpenSCManager(ScmAccessRights.Connect);
                try
                {
                    IntPtr service = Win32Api.OpenService(scm, this._serviceName, ServiceAccessRights.QueryStatus);
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
                            Win32Api.CloseServiceHandle(service);
                        }
                    }
                }
                finally
                {
                    Win32Api.CloseServiceHandle(scm);
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
                    IntPtr service = Win32Api.OpenService(scm, this._serviceName, ServiceAccessRights.QueryStatus);
                    ret = (service != IntPtr.Zero);
                    Win32Api.CloseServiceHandle(service);
                }
                finally
                {
                    Win32Api.CloseServiceHandle(scm);
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
                IntPtr service = Win32Api.OpenService(scm, this._serviceName, ServiceAccessRights.AllAccess);
                if (service == IntPtr.Zero)
                    throw new ServiceNotInstalledException(this._serviceName);

                try
                {
                    StopService(service);
                    if (!Win32Api.DeleteService(service))
                    {
                        var err = Marshal.GetLastWin32Error();
                        if (err != Win32Api.ERROR_SERVICE_MARKED_FOR_DELETE)
                        {
                            var lastError = new Win32Exception(err);
                            throw new ServiceOperationException(this._serviceName, "Delete", lastError.ErrorCode.ToString() + ": " + lastError.Message);
                        }
                    }
                }
                finally
                {
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        public int ServicePID
        {
            get
            {
                IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
                IntPtr service = Win32Api.OpenService(scm, this._serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                IntPtr buf = IntPtr.Zero;
                try
                {
                int size = 0;
                    Win32Api.QueryServiceStatusEx(service, 0, buf, size, out size);
                buf = Marshal.AllocHGlobal(size);
                if (!Win32Api.QueryServiceStatusEx(service, 0, buf, size, out size))
                {
                    return -1;
                }
                var status = (Win32Api.SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(buf, typeof(Win32Api.SERVICE_STATUS_PROCESS));
                return status.ProcessID;
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
                IntPtr service = Win32Api.OpenService(scm, this._serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    service = this.DoServiceInstall(scm);

                try
                {
                    this.StartService(service);
                }
                finally
                {
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        public void Install()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                Win32Api.CloseServiceHandle(DoServiceInstall(scm));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        private IntPtr DoServiceInstall(IntPtr scm)
        {
            if (this.ServiceExe == null)
                throw new ServiceOperationException(this._serviceName, "Install", "no ServiceExe set");

            if (!File.Exists(this.ServiceExe))
                throw new ServiceOperationException(this._serviceName, "Install", "Can't find service executable at: " + this._serviceExe);

            var service = Win32Api.CreateService(scm, this._serviceName, this._displayName, ServiceAccessRights.AllAccess, Win32Api.SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.AutoStart, ServiceError.Normal,
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
                IntPtr service = Win32Api.OpenService(scm, this._serviceName,
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
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        public void Stop(bool wait = true)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = Win32Api.OpenService(scm, this._serviceName,
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
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        public void Pause()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = Win32Api.OpenService(scm, this._serviceName,
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
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        public void Continue()
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr service = Win32Api.OpenService(scm, this._serviceName,
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
                    Win32Api.CloseServiceHandle(service);
                }
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }
        }

        private void StartService(IntPtr service, bool wait = true)
        {
            if (this.GetServiceStatus(service) == ServiceState.Running)
                return;
            Win32Api.StartService(service, 0, 0);
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
            Win32Api.SERVICE_STATUS status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Stop, status);
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
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Pause, status);
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
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Continue, status);
            if (wait)
            {
                var changedStatus = this.WaitForServiceStatus(service, ServiceState.ContinuePending, ServiceState.Running);
                if (!changedStatus)
                    throw new ServiceOperationException(this._serviceName, "Continue", "Unable to continue service");
            }
        }

        private ServiceState GetServiceStatus(IntPtr service)
        {
            Win32Api.SERVICE_STATUS status = new Win32Api.SERVICE_STATUS();

            if (Win32Api.QueryServiceStatus(service, status) == 0)
                throw new ServiceOperationException(this._serviceName, "GetServiceStatus", "Failed to query service status");
            return status.CurrentState;
        }

        private bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.QueryServiceStatus(service, status);
            if (status.CurrentState == desiredStatus) return true;

            WaitForServiceToChangeStatusTo(service, waitStatus, status);

            if ((status.CurrentState != desiredStatus) && (this.ServiceStateExtraWaitSeconds > 0))
            {
                var waited = 0;
                while ((waited < this.ServiceStateExtraWaitSeconds) && (status.CurrentState != desiredStatus))
                {
                    Thread.Sleep(1000);
                    waited++;
                    if (Win32Api.QueryServiceStatus(service, status) == 0) break;
                }
            }
            if (status.CurrentState != desiredStatus)
                return KillService();
            return (status.CurrentState == desiredStatus);
        }

        private static void WaitForServiceToChangeStatusTo(IntPtr service, ServiceState waitStatus, Win32Api.SERVICE_STATUS current)
        {
            var startTickCount = Environment.TickCount;
            var oldCheckPoint = current.CheckPoint;

            var waitTime = DetermineWaitTimeFor(current);

            while (current.CurrentState == waitStatus)
            {
                Thread.Sleep(waitTime);
                // Check the status again.
                if (Win32Api.QueryServiceStatus(service, current) == 0) break;
                if (current.CheckPoint > oldCheckPoint)
                {
                    // The service is making progress.
                    startTickCount = Environment.TickCount;
                    oldCheckPoint = current.CheckPoint;
                }
                else
                {
                    if (Environment.TickCount - startTickCount > current.WaitHint)
                    {
                        // No progress made within the wait hint
                        break;
                    }
                }
            }
        }

        private static int DetermineWaitTimeFor(Win32Api.SERVICE_STATUS status)
        {
// Do not wait longer than the wait hint. A good interval is
            // one tenth the wait hint, but no less than 1 second and no
            // more than 10 seconds.
            var waitTime = status.WaitHint/10;

            if (waitTime < 1000) waitTime = 1000;
            else if (waitTime > 10000) waitTime = 10000;
            return waitTime;
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
            IntPtr scm = Win32Api.OpenSCManager(null, null, rights);
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
