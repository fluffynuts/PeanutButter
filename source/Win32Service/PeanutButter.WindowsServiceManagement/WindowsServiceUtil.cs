using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Win32;
using PeanutButter.WindowsServiceManagement.Exceptions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.WindowsServiceManagement
{
    public interface IWindowsServiceUtil
    {
        string ServiceExe { get; }
        ServiceState State { get; }
        bool IsInstalled { get; }
        int ServiceStateExtraWaitSeconds { get; set; }
        ServiceStartupTypes StartupType { get; }

        void Uninstall(bool waitForUninstall = false);
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
        private const string SERVICE_NOT_INSTALLED = "Service not installed";

        private readonly string _serviceName;

        public string ServiceName => _serviceName;

        public bool IsStoppable => StoppableStates.Contains(State);

        private static readonly HashSet<ServiceState> StoppableStates
            = new HashSet<ServiceState>(
                new[]
                {
                    ServiceState.ContinuePending,
                    ServiceState.PausePending,
                    ServiceState.Paused,
                    ServiceState.Running,
                    ServiceState.StartPending
                });

        public bool IsStartable => StartableStates.Contains(State);

        private static readonly HashSet<ServiceState> StartableStates
            = new HashSet<ServiceState>(
                new[]
                {
                    ServiceState.Stopped,
                    ServiceState.PausePending,
                    ServiceState.Paused,
                    ServiceState.Running,
                    ServiceState.StartPending
                });

        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value ?? _serviceName;
        }

        private string _displayName;
        private string _serviceExe;

        public string ServiceExe
            => _serviceExe ?? (_serviceExe = QueryExeForServiceByName(ServiceName));

        private static string QueryExeForServiceByName(string name)
        {
            var queryString =
                $"select PathName from Win32_Service where Name = '{name?.Replace("'", "''")}'";
            var searcher = new ManagementObjectSearcher(queryString);
            var collection = searcher.Get();
            return collection.Cast<ManagementBaseObject>()
                .Select(o => o.Properties["PathName"].Value.ToString())
                .FirstOrDefault();
        }

        public static WindowsServiceUtil GetServiceByPath(string path)
        {
            var queryString =
                $"select Name from Win32_Service where PathName = '{path.Replace("'", "''").Replace("\\", "\\\\")}'";
            var searcher = new ManagementObjectSearcher(queryString);
            var collection = searcher.Get();
            if (collection.Count == 0)
                return null;
            var svcName = collection.Cast<ManagementBaseObject>()
                .Select(item => item.Properties["Name"].Value.ToString())
                .FirstOrDefault();
            return svcName == null
                ? null
                : new WindowsServiceUtil(svcName);
        }

        public int ServiceStateExtraWaitSeconds { get; set; }

        public ServiceState State
        {
            get
            {
                ServiceState ret;
                var scm = OpenSCManager(ScmAccessRights.Connect);
                try
                {
                    var service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.QueryStatus);
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
                try
                {
                    var services = ServiceController.GetServices();
                    return services.Any(s => s.ServiceName == _serviceName);
                }
                catch
                {
                    return Win32ApiMethodForQueryingServiceInstalled();
                }
            }
        }

        private bool Win32ApiMethodForQueryingServiceInstalled()
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);
            bool ret;
            try
            {
                var service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.QueryStatus);
                ret = service != IntPtr.Zero;
                Win32Api.CloseServiceHandle(service);
            }
            finally
            {
                Win32Api.CloseServiceHandle(scm);
            }

            return ret;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public uint ServiceStopTimeoutMs { get; set; }

        public WindowsServiceUtil(
            string serviceName, 
            string displayName = null,
            string serviceExe = null)
        {
            _serviceName = serviceName;
            _displayName = displayName ?? _serviceName;
            _serviceExe = serviceExe;
        }

        public void Uninstall(bool waitForUninstall = false)
        {
            var scm = OpenSCManager(ScmAccessRights.AllAccess);

            try
            {
                var service = Win32Api.OpenService(scm, _serviceName, ServiceAccessRights.AllAccess);
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
                            throw new ServiceOperationException(_serviceName, ServiceOperationNames.UNINSTALL,
                                lastError.ErrorCode + ": " + lastError.Message);
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

            if (waitForUninstall)
                SleepWhilstInstalled();
        }

        private void SleepWhilstInstalled()
        {
            do
            {
                Thread.Sleep(1000);
            } while (IsInstalled);
        }

        // ReSharper disable once InconsistentNaming
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

                    var status =
                        (Win32Api.SERVICE_STATUS_PROCESS) Marshal.PtrToStructure(buf,
                            typeof(Win32Api.SERVICE_STATUS_PROCESS));
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
                IntPtr service = Win32Api.OpenService(
                    scm,
                    _serviceName,
                    ServiceAccessRights.AllAccess
                );

                if (service == IntPtr.Zero)
                {
                    service = DoServiceInstall(scm);
                }

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
            VerifyServiceExecutable();

            var service = Win32Api.CreateService(
                scm,
                _serviceName,
                _displayName,
                ServiceAccessRights.AllAccess,
                Win32Api.SERVICE_WIN32_OWN_PROCESS,
                ServiceBootFlag.AutoStart, ServiceError.Normal,
                _serviceExe, null, IntPtr.Zero, null, null, null);
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            if (service == IntPtr.Zero)
            {
                throw new ServiceOperationException(_serviceName, ServiceOperationNames.INSTALL,
                    $"Failed to install with executable: {_serviceExe}\nMore info:\n{win32Exception.Message}"
                );
            }

            return service;
        }

        private void VerifyServiceExecutable()
        {
            if (ServiceExe == null)
                throw new ServiceOperationException(_serviceName, ServiceOperationNames.INSTALL,
                    "no ServiceExe set");

            if (!File.Exists(ServiceExe))
                throw new ServiceOperationException(_serviceName, ServiceOperationNames.INSTALL,
                    "Can't find service executable at: " + _serviceExe);
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
                        Enum.Parse(typeof(ServiceStartupTypes), regKey.GetValue("Start").ToString(), true);
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
                var subKeypath = string.Join("\\", "SYSTEM", "CurrentControlSet", "Services", ServiceName);
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
                var service = Win32Api.OpenService(scm, _serviceName,
                    ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                if (service == IntPtr.Zero)
                    throw new ServiceOperationException(_serviceName, ServiceOperationNames.START,
                        SERVICE_NOT_INSTALLED);
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
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.STOP,
                        SERVICE_NOT_INSTALLED);

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
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.PAUSE,
                        SERVICE_NOT_INSTALLED);
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
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.PAUSE,
                        SERVICE_NOT_INSTALLED);
                try
                {
                    ContinueService(service);
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
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.START,
                        "Unable to start service");
            }
        }

        private void StopService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
                return;
            var process = Process.GetProcessById(ServicePID);
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Stop, status);
            if (wait)
            {
                WaitForServiceToStop(service, process);
            }
        }

        private void WaitForServiceToStop(IntPtr service, Process process)
        {
            var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
            if (!changedStatus)
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.STOP,
                    "Unable to stop service");
            var waitLevel = 0;
            try
            {
                if (!process.HasExited)
                {
                    waitLevel++;
                    if (!process.WaitForExit((int) ServiceStopTimeoutMs))
                    {
                        waitLevel++;
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowAppropriateExceptionFor(waitLevel, ServiceOperationNames.STOP, ex);
            }
        }

        private void ThrowAppropriateExceptionFor(int waitLevel, string operation, Exception ex)
        {
            switch (waitLevel)
            {
                case 0:
                    throw new ServiceOperationException(_serviceName, operation,
                        GenerateStopExceptionMessageWith("but threw errors when interrogating process state", ex));
                case 1:
                    throw new ServiceOperationException(_serviceName, operation,
                        GenerateStopExceptionMessageWith(
                            "and we got an error whilst waiting 10 seconds for graceful exit", ex));
                case 2:
                    throw new ServiceOperationException(_serviceName, operation,
                        GenerateStopExceptionMessageWith(
                            "and we got an error after trying to kill it when it didn't gracefully exit within 10 seconds",
                            ex));
            }
        }

        private string GenerateStopExceptionMessageWith(string subMessage, Exception ex)
        {
            return $"Service responded to stop command {subMessage} ({ex.Message})";
        }

        private void PauseService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.PAUSE,
                    "Cannot pause a service which isn't already running");
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Pause, status);
            if (wait)
            {
                var changedStatus = WaitForServiceStatus(service, ServiceState.PausePending, ServiceState.Paused);
                if (!changedStatus)
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.PAUSE,
                        "Unable to pause service");
            }
        }

        public void ContinueService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Paused)
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.CONTINUE,
                    "Cannot continue a service not in the paused state");
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
            var status = new Win32Api.SERVICE_STATUS();

            if (Win32Api.QueryServiceStatus(service, status) == 0)
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.GET_SERVICE_STATUS,
                    "Failed to query service status");
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

        private static void WaitForServiceToChangeStatusTo(IntPtr service,
            ServiceState waitStatus,
            Win32Api.SERVICE_STATUS current)
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
            var waitTime = status.WaitHint / 10;

            if (waitTime < 1000) waitTime = 1000;
            if (waitTime > 10000) waitTime = 10000;
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
                        catch
                        {
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    // happens if a 32-bit process tries to read a 64-bit process' info
                    if (proc.ProcessName.ToLower() == Path.GetFileName(ServiceExe)?.ToLower())
                        killThese.Add(proc);
                }
            }

            if (killThese.Count == 0)
                return true; // not running any more
            int killed = 0;
            foreach (var proc in killThese)
            {
                try
                {
                    if (!proc.HasExited)
                        proc.Kill();
                    killed++;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    /* intentionally left blank */
                }
            }

            return (killed == killThese.Count);
        }

        // ReSharper disable once InconsistentNaming
        private IntPtr OpenSCManager(ScmAccessRights rights)
        {
            var scm = Win32Api.OpenSCManager(null, null, rights);
            if (scm == IntPtr.Zero)
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.OPEN_SC_MANAGER,
                    "Could not connect to service control manager");

            return scm;
        }
    }
}