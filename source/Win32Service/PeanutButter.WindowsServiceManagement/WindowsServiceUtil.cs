using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;
using PeanutButter.WindowsServiceManagement.Exceptions;
using Imported.PeanutButter.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.WindowsServiceManagement
{
    public interface IWindowsServiceUtil
    {
        string Commandline { get; }
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

        // TODO: when displayname is not set for install (ie from query)
        //         then this util should query for the current display name of
        //         the service (if found)
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value ?? _serviceName;
        }

        private string _displayName;
        private readonly string _serviceCommandline;
        private string _serviceExe;

        public string Commandline => _serviceCommandline;

        public string ServiceExe
            => _serviceExe ??= QueryExeForServiceName(ServiceName);


        private static string QueryExeForServiceName(string name)
        {
            return FindExecutableForServiceByName(name);
        }

        private static string QueryCommandlineForServiceName(string name)
        {
            return QueryServiceControlForValueOfLineStartingWith(
                ScPrefixes.SERVICE_CLI,
                name
            );
        }

        private static string QueryServiceControlForValueOfLineStartingWith(
            string prefix,
            string serviceName)
        {
            using var io = new ProcessIO("sc", "qc", $"\"{serviceName}\"");
            var interestingLine = io.StandardOutput
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.StartsWith(prefix));
            return interestingLine == null
                ? null
                : string.Join(":",
                    interestingLine.Split(':')
                        .Skip(1)
                ).Trim();
        }

        private static string QueryDisplayNameForServiceName(string name)
        {
            return QueryServiceControlForValueOfLineStartingWith(
                ScPrefixes.DISPLAY_NAME,
                name
            );
        }

        private static string FindExecutableForServiceByName(
            string name)
        {
            var cli = QueryCommandlineForServiceName(name);
            return ProgramFinder.Matches(cli).OfType<Match>()
                .FirstOrDefault()
                ?.Value;
        }

        private static readonly Regex ProgramFinder = new Regex("((\".+\")|([^ ]+))");

        private static class ScPrefixes
        {
            public const string DISPLAY_NAME = "DISPLAY_NAME";
            public const string SERVICE_NAME = "SERVICE_NAME";
            public const string SERVICE_CLI = "BINARY_PATH_NAME";
        }

        public static WindowsServiceUtil GetServiceByPath(string path)
        {
            var queryString =
                $"select Name from Win32_Service where PathName = '{path.Replace("'", "''").Replace("\\", "\\\\")}'";
            using var searcher = new ManagementObjectSearcher(queryString);
            using var collection = searcher.Get();
            if (collection.Count == 0)
            {
                return null;
            }

            var svcName = collection.Cast<ManagementBaseObject>()
                .Select(item => item.Properties["Name"].Value.ToString())
                .FirstOrDefault();
            return svcName == null
                ? null
                : new WindowsServiceUtil(svcName);
            
            // TODO: this works, but it's SLOW
            // -> try rather querying the registry directly:
            //  HKLM/CurrentControlSet/Services/(service-name)/ImagePath
            //  where type (DWORD) == 16

            // using (var io = new ProcessIO("sc", "query", "type=", "service"))
            // {
            //     var output = io.StandardOutput.ToArray();
            //     var serviceName = output
            //         .Select(line => line.Trim())
            //         .Where(line => line.StartsWith(ScPrefixes.SERVICE_NAME))
            //         .Select(line => string.Join(":", line.Split(':').Skip(1)).Trim())
            //         .Select(name => new { name, exe = FindExecutableForServiceByName(name) }).Where(o => path.Equals(o.exe, StringComparison.OrdinalIgnoreCase))
            //         .Select(o => o.name)
            //         .FirstOrDefault();
            //     return serviceName == null
            //         ? null as WindowsServiceUtil
            //         : new WindowsServiceUtil(serviceName);
            //         
            // }
        }

        public int ServiceStateExtraWaitSeconds { get; set; }

        public ServiceState State
        {
            get
            {
                return TryDoWithService(
                    service => service == IntPtr.Zero
                        ? ServiceState.NotFound
                        : GetServiceStatus(service)
                );
            }
        }

        public bool IsInstalled =>
            Win32ApiMethodForQueryingServiceInstalled();

        private bool Win32ApiMethodForQueryingServiceInstalled()
        {
            return TryDoWithService(service => service != IntPtr.Zero);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public uint ServiceStopTimeoutMs { get; set; }

        public WindowsServiceUtil(
            string serviceName,
            string displayName = null,
            string serviceCommandline = null)
        {
            _serviceName = serviceName;
            _displayName = displayName 
                // when not given a display name, assume in query mode
                ?? QueryDisplayNameForServiceName(serviceName) 
                // fall back on service name -- at least registration won't b0rk
                ?? serviceName;
            _serviceCommandline = serviceCommandline 
                ?? QueryCommandlineForServiceName(serviceName);
        }

        public void Uninstall()
        {
            Uninstall(false);
        }

        public void Uninstall(bool waitForUninstall)
        {
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    throw new ServiceNotInstalledException(_serviceName);
                }

                StopService(service);
                if (Win32Api.DeleteService(service))
                {
                    return;
                }

                var err = Marshal.GetLastWin32Error();
                if (err == Win32Api.ERROR_SERVICE_MARKED_FOR_DELETE)
                {
                    return;
                }

                var lastError = new Win32Exception(err);
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.UNINSTALL,
                    $"{lastError.ErrorCode}: {lastError.Message}"
                );
            });

            if (waitForUninstall)
            {
                SleepWhilstInstalled();
            }
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
                return TryDoWithService(service =>
                {
                    var buf = IntPtr.Zero;
                    try
                    {
                        var size = 0;
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
                        {
                            Marshal.FreeHGlobal(buf);
                        }
                    }
                });
            }
        }

        public void InstallAndStart()
        {
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    TryDoWithSCManager(scm =>
                    {
                        service = DoServiceInstall(scm);
                    });
                }

                StartService(service);
            });
        }

        private void TryDoWithService(Action<IntPtr> toRun)
        {
            TryDoWithService(service =>
            {
                toRun(service);
                return 0;
            });
        }

        private T TryDoWithService<T>(Func<IntPtr, T> toRun)
        {
            return TryDoWithSCManager(scm =>
            {
                var service = IntPtr.Zero;
                try
                {
                    service = Win32Api.OpenService(
                        scm,
                        _serviceName,
                        ServiceAccessRights.AllAccess
                    );
                    return toRun(service);
                }
                finally
                {
                    if (service != IntPtr.Zero)
                    {
                        Win32Api.CloseServiceHandle(service);
                    }
                }
            });
        }

        private T TryDoWithSCManager<T>(Func<IntPtr, T> toRun)
        {
            var scm = IntPtr.Zero;
            try
            {
                scm = OpenSCManager(ScmAccessRights.AllAccess);
                return toRun(scm);
            }
            finally
            {
                if (scm != IntPtr.Zero)
                {
                    Win32Api.CloseServiceHandle(scm);
                }
            }
        }

        private void TryDoWithSCManager(Action<IntPtr> toRun)
        {
            TryDoWithSCManager(scm =>
            {
                toRun(scm);
                return 0;
            });
        }

        public void Install()
        {
            TryDoWithSCManager(scm =>
            {
                try
                {
                    Win32Api.CloseServiceHandle(
                        DoServiceInstall(scm)
                    );
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            });
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
                _serviceCommandline, null, IntPtr.Zero, null, null, null);
            var win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            if (service == IntPtr.Zero)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.INSTALL,
                    $"Failed to install with executable: {_serviceCommandline}\nMore info:\n{win32Exception.Message}"
                );
            }

            return service;
        }

        private void VerifyServiceExecutable()
        {
            if (string.IsNullOrWhiteSpace(Commandline))
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.INSTALL,
                    "no ServiceExe set"
                );
            }

            var parts = Commandline.Split(new[] { " " }, StringSplitOptions.None);
            var collected = new List<string>();
            var skipped = 0;
            do
            {
                collected.Add(parts.Skip(skipped).First());
                skipped++;
            } while (skipped < parts.Length && collected.Aggregate(
                0,
                (acc, cur) => cur.Count(c => c == '"') + acc
            ) % 2 == 1);

            var executable = string.Join(" ", collected);

            if (!File.Exists(executable))
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.INSTALL,
                    "Can't find service executable at: " + _serviceCommandline
                );
            }
        }

        public ServiceStartupTypes StartupType
        {
            get
            {
                using var regKey = GetServiceRegistryKey();
                if (!regKey.Exists)
                {
                    return ServiceStartupTypes.Unknown;
                }

                return Enum.TryParse(
                    regKey.GetValue("Start").ToString(),
                    true,
                    out ServiceStartupTypes result)
                    ? result
                    : ServiceStartupTypes.Unknown;
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
            if (!IsInstalled)
            {
                return;
            }

            using var regKey = GetServiceRegistryKey();
            if (!regKey.Exists)
            {
                throw new Exception($"Unable to open service registry key for '{ServiceName}'");
            }

            regKey.SetValue("Start", value, RegistryValueKind.DWord);
        }

        private RegKey GetServiceRegistryKey()
        {
            try
            {
                var path = string.Join("\\", "SYSTEM", "CurrentControlSet", "Services", ServiceName);
                return new RegKey(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Start(bool wait = true)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

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
            var scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                var service = Win32Api.OpenService(scm, _serviceName,
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
            var scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                var service = Win32Api.OpenService(scm, _serviceName,
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
            var scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                var service = Win32Api.OpenService(scm, _serviceName,
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

        private void StopService(
            IntPtr service,
            bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
            {
                return;
            }

            using var process = Process.GetProcessById(ServicePID);
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
            {
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.STOP,
                    "Unable to stop service");
            }

            var waitLevel = 0;
            try
            {
                if (process.HasExited)
                {
                    return;
                }

                waitLevel++;
                if (process.WaitForExit((int) ServiceStopTimeoutMs))
                {
                    return;
                }

                waitLevel++;
                process.Kill();
            }
            catch (Exception ex)
            {
                ThrowAppropriateExceptionFor(waitLevel, ServiceOperationNames.STOP, ex);
            }
        }

        private int ThrowAppropriateExceptionFor(int waitLevel, string operation, Exception ex)
        {
            switch (waitLevel)
            {
                case 0:
                    return DoThrow("but threw errors when interrogating process state");
                case 1:
                    return DoThrow("and we got an error whilst waiting 10 seconds for graceful exit");
                default:
                    return DoThrow(
                        "and we got an error after trying to kill it when it didn't gracefully exit within 10 seconds");
            }

            int DoThrow(string message)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    operation,
                    $"Service responded to stop command {message} ({ex.Message})"
                );
            }
        }

        private void PauseService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Running)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.PAUSE,
                    "Cannot pause a service which isn't already running"
                );
            }

            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Pause, status);
            if (!wait)
            {
                return;
            }

            var changedStatus = WaitForServiceStatus(
                service,
                ServiceState.PausePending,
                ServiceState.Paused
            );
            if (!changedStatus)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.PAUSE,
                    "Unable to pause service"
                );
            }
        }

        public void ContinueService(IntPtr service, bool wait = true)
        {
            if (GetServiceStatus(service) != ServiceState.Paused)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.CONTINUE,
                    "Cannot continue a service not in the paused state"
                );
            }

            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.ControlService(service, ServiceControl.Continue, status);
            if (!wait)
            {
                return;
            }

            var changedStatus = WaitForServiceStatus(
                service,
                ServiceState.ContinuePending,
                ServiceState.Running
            );
            if (!changedStatus)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.CONTINUE,
                    "Unable to continue service"
                );
            }
        }

        private ServiceState GetServiceStatus(IntPtr service)
        {
            var status = new Win32Api.SERVICE_STATUS();

            if (Win32Api.QueryServiceStatus(service, status) == 0)
            {
                throw new ServiceOperationException(_serviceName,
                    ServiceOperationNames.GET_SERVICE_STATUS,
                    "Failed to query service status"
                );
            }

            return status.CurrentState;
        }

        private bool WaitForServiceStatus(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus)
        {
            var status = new Win32Api.SERVICE_STATUS();
            Win32Api.QueryServiceStatus(service, status);
            if (status.CurrentState == desiredStatus)
            {
                return true;
            }

            WaitForServiceToChangeStatusTo(service, waitStatus, status);

            if (status.CurrentState == desiredStatus)
            {
                return true;
            }

            if (ServiceStateExtraWaitSeconds > 0)
            {
                var waited = 0;
                while (waited < ServiceStateExtraWaitSeconds &&
                    status.CurrentState != desiredStatus)
                {
                    Thread.Sleep(1000);
                    waited++;
                    if (Win32Api.QueryServiceStatus(service, status) == 0)
                    {
                        break;
                    }
                }
            }

            return status.CurrentState == desiredStatus ||
                KillService();
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
            using var myProc = Process.GetCurrentProcess();
            foreach (var proc in Process.GetProcesses())
            {
                using (proc)
                {
                    if (proc.Id == myProc.Id)
                    {
                        continue;
                    }

                    try
                    {
                        if (proc?.MainModule?.FileName.ToLower() != Commandline)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        // happens if a 32-bit process tries to read a 64-bit process' info
                        if (proc.ProcessName.ToLower() == Path.GetFileName(Commandline)?.ToLower())
                        {
                            killThese.Add(proc);
                        }
                    }

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

            if (killThese.Count == 0)
            {
                return true; // not running any more
            }

            var killed = 0;
            foreach (var proc in killThese)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }

                    killed++;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    /* intentionally left blank */
                }
            }

            return killed == killThese.Count;
        }

        // ReSharper disable once InconsistentNaming
        private IntPtr OpenSCManager(ScmAccessRights rights)
        {
            var scm = Win32Api.OpenSCManager(null, null, rights);
            if (scm == IntPtr.Zero)
            {
                throw new ServiceOperationException(
                    _serviceName,
                    ServiceOperationNames.OPEN_SC_MANAGER,
                    "Could not connect to service control manager"
                );
            }

            return scm;
        }
    }
}