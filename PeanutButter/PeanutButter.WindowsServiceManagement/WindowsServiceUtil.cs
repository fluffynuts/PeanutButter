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

    public class WindowsServiceUtil : IWindowsServiceUtil
    {
        private const string _serviceNotInstalled = "Service not installed";

        private string _serviceName;
        public string ServiceName { get { return _serviceName; } }
        public string DisplayName 
        { 
            get
            {
                return _displayName;
            }
            set
            {
                if (value == null)
                    _displayName = _serviceName;
                else
                    _displayName = value;
            }
        }
        private string _displayName;
        private string _serviceExe;

        public string ServiceExe
        {
            get
            {
                if (_serviceExe != null)
                    return _serviceExe;

                var searcher = new ManagementObjectSearcher(string.Format("select PathName from Win32_Service where Name = '{0}'",
                                                                           _serviceName.Replace("'", "''")));
                var collection = searcher.Get();
                if (collection.Count == 0)
                    return null;
                foreach (var svc in collection)
                {
                    _serviceExe = svc["PathName"].ToString();
                    break;
                }
                return _serviceExe;
            }
            set 
            {
                _serviceExe = value;
            }
        }

        public static WindowsServiceUtil GetServiceByPath(string path)
        {
            var searcher = new ManagementObjectSearcher(string.Join(string.Empty,
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
                IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
                try
                {
                    IntPtr service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.QueryStatus);
                    if (service == IntPtr.Zero)
                        ret = ServiceState.NotFound;
                    else
                    {
                        try
                        {
                            ret = GetServiceStatus(service);
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
                IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
                var ret = false;
                try
                {
                    IntPtr service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.QueryStatus);
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
            get { return _serviceStopTimeoutMS; }
            set { _serviceStopTimeoutMS = value; }
        }

        public WindowsServiceUtil(string serviceName, string displayName = null, string defaultServiceExePath = null)
        {
            _serviceName = serviceName;
            _displayName = displayName;
            if (_displayName == null)
                _displayName = _serviceName;
            if (ServiceExe == null)    // see if we can get an existing path else set to the defaultServicePath for installations
                ServiceExe = defaultServiceExePath;
        }

        public void Uninstall(bool waitForUninstall = true)
        {
            IntPtr scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                IntPtr service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.AllAccess);
                if (service == IntPtr.Zero)
                    throw new ServiceNotInstalledException(_serviceName);
                try
                {
                    StopService(service);
                    if (!Win32Api.DeleteService(service))
                    {
                        var err = Marshal.GetLastWin32Error();
                        if (err != Win32Api.ERROR_SERVICE_MARKED_FOR_DELETE)
                        {
                            var lastError = new Win32Exception(err);
                            throw new ServiceOperationException(_serviceName, "Delete", lastError.ErrorCode.ToString() + ": " + lastError.Message);
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

            if(waitForUninstall)
                SleepWhilstInstalled();
        }

        private void SleepWhilstInstalled()
        {
            do
            {
                Thread.Sleep(1000);
            } while (IsInstalled);
        }

        public int ServicePID
        {
            get
            {
                IntPtr scm = OpenSCManager(ScmAccessRights.Connect);
                IntPtr service = Win32Api.OpenService(scm, _serviceName,
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
                IntPtr service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    service = DoServiceInstall(scm);

                try
                {
                    StartService(service);
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
            if (ServiceExe == null)
                throw new ServiceOperationException(_serviceName, "Install", "no ServiceExe set");

            if (!File.Exists(ServiceExe))
                throw new ServiceOperationException(_serviceName, "Install", "Can't find service executable at: " + _serviceExe);

            var service = Win32Api.CreateService(scm, _serviceName, _displayName, ServiceAccessRights.AllAccess, Win32Api.SERVICE_WIN32_OWN_PROCESS, ServiceBootFlag.AutoStart, ServiceError.Normal,
                                    _serviceExe, null, IntPtr.Zero, null, null, null);
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            if (service == IntPtr.Zero)
                throw new ServiceOperationException(_serviceName, "Install", 
                    string.Join(string.Empty, new string[] { "Failed to install with executable: " + _serviceExe, "\nMore info:\n", win32Exception.Message }));

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
                var subKeypath = string.Join("\\", new[] { 
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
                IntPtr service = Win32Api.OpenService(scm, _serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(_serviceName, "Start", _serviceNotInstalled);
                try
                {
                    StartService(service, wait);
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
                IntPtr service = Win32Api.OpenService(scm, _serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(_serviceName, "Stop", _serviceNotInstalled);

                try
                {
                    StopService(service, wait);
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
                IntPtr service = Win32Api.OpenService(scm, _serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(_serviceName, "Pause", _serviceNotInstalled);
                try
                {
                    PauseService(service);
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
                IntPtr service = Win32Api.OpenService(scm, _serviceName,
                                             ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(_serviceName, "Pause", _serviceNotInstalled);
                try
                {
                    PauseService(service);
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
            if (GetServiceStatus(service) == ServiceState.Running)
                return;
            Win32Api.StartService(service, 0, 0);
            if (wait)
            {
                var changedStatus = WaitForServiceStatus(service, ServiceState.StartPending, ServiceState.Running);
                if (!changedStatus)
                    throw new ServiceOperationException(_serviceName, "Start", "Unable to start service");
            }
        }

        private void StopService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
                return;
            var process = Process.GetProcessById(ServicePID);
            Win32Api.SERVICE_STATUS status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Stop, status);
            if (wait)
            {
                WaitForServiceToStop(service, process);
            }
        }

        private void WaitForServiceToStop(IntPtr service, Process process)
        {
            const string op = "Stop";
            var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
            if (!changedStatus)
                throw new ServiceOperationException(_serviceName, op, "Unable to stop service");
            var waitLevel = 0;
            try
            {
                if (!process.HasExited)
                {
                    waitLevel++;
                    if (!process.WaitForExit((int) _serviceStopTimeoutMS))
                    {
                        waitLevel++;
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowAppropriateExceptionFor(waitLevel, op, ex);
            }
        }

        private void ThrowAppropriateExceptionFor(int waitLevel, string op, Exception ex)
        {
            switch (waitLevel)
            {
                case 0:
                    throw new ServiceOperationException(_serviceName, op,
                        GenerateStopExceptionMessageWith("but threw errors when interrogating process state", ex));
                case 1:
                    throw new ServiceOperationException(_serviceName, op,
                        GenerateStopExceptionMessageWith("and we got an error whilst waiting 10 seconds for graceful exit", ex));
                case 2:
                    throw new ServiceOperationException(_serviceName, op,
                        GenerateStopExceptionMessageWith(
                            "and we got an error after trying to kill it when it didn't gracefully exit within 10 seconds", ex));
            }
        }

        private string GenerateStopExceptionMessageWith(string subMessage, Exception ex)
        {
            return string.Format("{0} {1} ({2})",
                "Service responded to stop command",
                subMessage,
                ex.Message);
        }

        private void PauseService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
                throw new ServiceOperationException(_serviceName, "Pause", "Cannot pause a service which isn't already running");
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Pause, status);
            if (wait)
            {
                var changedStatus = WaitForServiceStatus(service, ServiceState.PausePending, ServiceState.Paused);
                if (!changedStatus)
                    throw new ServiceOperationException(_serviceName, "Pause", "Unable to pause service");
            }
        }

        public void ContinueService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Paused)
                throw new ServiceOperationException(_serviceName, "Continue", "Cannot continue a service not in the paused state");
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Continue, status);
            if (wait)
            {
                var changedStatus = WaitForServiceStatus(service, ServiceState.ContinuePending, ServiceState.Running);
                if (!changedStatus)
                    throw new ServiceOperationException(_serviceName, "Continue", "Unable to continue service");
            }
        }

        private ServiceState GetServiceStatus(IntPtr service)
        {
            Win32Api.SERVICE_STATUS status = new Win32Api.SERVICE_STATUS();

            if (Win32Api.QueryServiceStatus(service, status) == 0)
                throw new ServiceOperationException(_serviceName, "GetServiceStatus", "Failed to query service status");
            return status.CurrentState;
        }

        private bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.QueryServiceStatus(service, status);
            if (status.CurrentState == desiredStatus) return true;

            WaitForServiceToChangeStatusTo(service, waitStatus, status);

            if ((status.CurrentState != desiredStatus) && (ServiceStateExtraWaitSeconds > 0))
            {
                var waited = 0;
                while ((waited < ServiceStateExtraWaitSeconds) && (status.CurrentState != desiredStatus))
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
                    if (proc.MainModule.FileName.ToLower() == ServiceExe)
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
                    if (proc.ProcessName.ToLower() == Path.GetFileName(ServiceExe).ToLower())
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
                throw new ServiceOperationException(_serviceName, "OpenSCManager", "Could not connect to service control manager");

            return scm;
        }

    }
}
