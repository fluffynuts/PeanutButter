using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
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
        string ServiceName { get; }
        string DisplayName { get; }
        string Commandline { get; }
        ServiceState State { get; }
        bool IsInstalled { get; }
        bool IsMarkedForDelete { get; }
        int ServiceStateExtraWaitSeconds { get; set; }
        ServiceStartupTypes StartupType { get; }
        int ServicePID { get; }

        void Uninstall();

        [Obsolete("Rather use the overload with ControlOptions")]
        void Uninstall(bool waitForUninstall);

        void Uninstall(ControlOptions options);
        void InstallAndStart();
        void InstallAndStart(bool waitForStart);
        void Install();
        void Start();
        void Start(bool wait);
        void Stop();

        [Obsolete("Rather use the overload with ControlOptions")]
        void Stop(bool wait);

        void Stop(ControlOptions options);
        void Pause();
        void Continue();
        void Disable();
        void SetAutomaticStart();
        void SetManualStart();
        KillServiceResult KillService();
    }

    [Flags]
    public enum ControlOptions
    {
        None = 0,
        Wait = 1,
        Force = 2
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

        public string Commandline =>
            _serviceCommandline ?? FindCommandlineForServiceName(ServiceName);

        public string ServiceExe
            => _serviceExe ??= QueryExeForServiceName(ServiceName);

        public string[] Arguments
            => _arguments ??= QueryServiceArguments(ServiceName);

        private string[] _arguments;

        private static string[] QueryServiceArguments(string name)
        {
            var cli = FindCommandlineForServiceName(name);
            var parts = cli.SplitCommandline();
            return parts.Skip(1).ToArray();
        }

        private static string QueryExeForServiceName(string name)
        {
            return FindExecutableForServiceByName(name);
        }

        private static string FindCommandlineForServiceName(string name)
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
            var cli = FindCommandlineForServiceName(name);
            return cli.SplitCommandline().FirstOrDefault();
        }

        private static class ScPrefixes
        {
            public const string DISPLAY_NAME = "DISPLAY_NAME";
            public const string SERVICE_NAME = "SERVICE_NAME";
            public const string SERVICE_CLI = "BINARY_PATH_NAME";
        }

        private const string REG_SERVICES_BASE = "SYSTEM\\CurrentControlSet\\Services";

        public static WindowsServiceUtil GetServiceByPath(string path)
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(REG_SERVICES_BASE);
            if (baseKey is null)
            {
                throw new SecurityException(
                    $"Unable to read registry key {REG_SERVICES_BASE}"
                );
            }

            var searchParts = path.SplitCommandline();
            var potentials = new List<string>(); // not direct matches
            foreach (var serviceName in baseKey.GetSubKeyNames())
            {
                using var subKey = baseKey.OpenSubKey(serviceName);
                var imagePath = subKey?.GetValue("ImagePath") as string;
                if (imagePath is null)
                {
                    continue;
                }

                var parts = imagePath.SplitCommandline();
                if (parts.Matches(searchParts, StringComparison.OrdinalIgnoreCase))
                {
                    return new WindowsServiceUtil(serviceName);
                }

                if (parts[0].Equals(searchParts[0], StringComparison.OrdinalIgnoreCase))
                {
                    potentials.Add(serviceName);
                }
            }

            // matched on service exe, even without args
            if (potentials.Count == 1)
            {
                return new WindowsServiceUtil(potentials[0]);
            }

            var msg = potentials.Count == 0
                ? "No"
                : "Multiple";
            throw new ArgumentException(
                $"{msg} matches for queried service path {path}"
            );
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

        public bool IsMarkedForDelete =>
            ServiceIsMarkedForDelete();

        private bool Win32ApiMethodForQueryingServiceInstalled()
        {
            return TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    return false; // definitely not installed
                }

                return !ServiceIsMarkedForDelete();
            });
        }

        private bool ServiceIsMarkedForDelete()
        {
            try
            {
                // we have to go old-school and expect stuff from the registry
                var key = Registry.LocalMachine.OpenSubKey(
                    $"SYSTEM\\CurrentControlSet\\Services\\{ServiceName}"
                );
                var deleteFlag = (int) key.GetValue("DeleteFlag");
                return deleteFlag != 0;
            }
            catch
            {
                // if we can't query, we can't definitively say it's marked for delete
                return false;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public uint ServiceStopTimeoutMs { get; set; }

        public WindowsServiceUtil(string serviceName)
            : this(serviceName, null, null)
        {
        }

        public WindowsServiceUtil(
            string serviceName,
            string displayName,
            string serviceCommandline)
        {
            _serviceName = serviceName;
            _displayName = displayName
                // when not given a display name, assume in query mode
                ?? QueryDisplayNameForServiceName(serviceName)
                // fall back on service name -- at least registration won't b0rk
                ?? serviceName;
            _serviceCommandline = serviceCommandline;
        }

        /// <summary>
        /// Uninstalls the service, waiting for it to stop and be uninstalled
        /// </summary>
        public void Uninstall()
        {
            Uninstall(true);
        }

        /// <summary>
        /// Uninstall the service, conditionally don't wait for the process to complete
        /// </summary>
        /// <param name="waitForUninstall"></param>
        /// <exception cref="ServiceNotInstalledException"></exception>
        /// <exception cref="ServiceOperationException"></exception>
        public void Uninstall(bool waitForUninstall)
        {
            Uninstall(ControlOptions.Wait | ControlOptions.Force);
        }

        public void Uninstall(ControlOptions options)
        {
            var wait = options.HasFlag(ControlOptions.Wait);
            var force = options.HasFlag(ControlOptions.Force);
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    throw new ServiceNotInstalledException(_serviceName);
                }

                StopService(service, wait, force);
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

            if (wait)
            {
                SleepWhilstInstalled();
            }
        }

        private void SleepWhilstInstalled()
        {
            while (IsInstalled)
            {
                Thread.Sleep(1000);
            }
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
            InstallAndStart(true);
        }

        public void InstallAndStart(bool waitForStart)
        {
            TryDoWithService(service =>
            {
                var installedHere = false;
                if (service == IntPtr.Zero)
                {
                    TryDoWithSCManager(scm =>
                    {
                        service = DoServiceInstall(scm);
                        if (service != IntPtr.Zero)
                        {
                            installedHere = true;
                        }
                    });
                }

                if (service == IntPtr.Zero)
                {
                    throw new ServiceNotInstalledException(
                        $"'{ServiceName}' not installed and unable to install"
                    );
                }

                StartService(service, waitForStart);
                if (installedHere)
                {
                    Win32Api.CloseServiceHandle(service);
                }
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
                Win32Api.CloseServiceHandle(
                    DoServiceInstall(scm)
                );
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
            // need to trim out the quotes, yo
            executable = executable.Trim('"');

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
            return ServiceRegistryKeyFor(ServiceName);
        }

        private RegKey ServiceRegistryKeyFor(string name)
        {
            try
            {
                var path = string.Join("\\", "SYSTEM", "CurrentControlSet", "Services", name);
                return new RegKey(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Starts the service and waits for it to enter running state
        /// </summary>
        public void Start()
        {
            Start(true);
        }

        /// <summary>
        /// Starts the service, optionally waiting for it to enter running state
        /// </summary>
        /// <param name="wait"></param>
        /// <exception cref="ServiceOperationException"></exception>
        public void Start(bool wait)
        {
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    throw new ServiceOperationException(_serviceName, ServiceOperationNames.START,
                        SERVICE_NOT_INSTALLED);
                }

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
            });
        }

        /// <summary>
        /// Stops the service, waiting for it to enter stopped state
        /// </summary>
        public void Stop()
        {
            Stop(true);
        }

        /// <summary>
        /// Stops the service, optionally waiting to enter stopped state
        /// </summary>
        /// <param name="wait"></param>
        /// <exception cref="ServiceOperationException"></exception>
        public void Stop(bool wait)
        {
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.STOP,
                        SERVICE_NOT_INSTALLED);
                }

                StopService(service, wait, true);
            });
        }

        public void Stop(ControlOptions options)
        {
            TryDoWithService(service =>
            {
                if (service == IntPtr.Zero)
                {
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.STOP,
                        SERVICE_NOT_INSTALLED);
                }

                StopService(
                    service,
                    // fixme
                    true, true);
            });
        }

        /// <summary>
        /// Pauses the service, waiting for it to enter paused state
        /// </summary>
        public void Pause()
        {
            Pause(true);
        }

        /// <summary>
        /// Pauses the service, optionally waiting for it to enter paused state
        /// </summary>
        /// <param name="wait"></param>
        /// <exception cref="ServiceOperationException"></exception>
        public void Pause(bool wait)
        {
            TryDoWithSCManager(scm =>
            {
                var service = Win32Api.OpenService(scm, _serviceName,
                    ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue);
                if (service == IntPtr.Zero)
                {
                    throw new ServiceOperationException(_serviceName,
                        ServiceOperationNames.PAUSE,
                        SERVICE_NOT_INSTALLED);
                }

                try
                {
                    PauseService(service, wait);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                finally
                {
                    Win32Api.CloseServiceHandle(service);
                }
            });
        }

        /// <summary>
        /// Continues a paused service, waiting for it to enter running state
        /// </summary>
        public void Continue()
        {
            Continue(true);
        }

        /// <summary>
        /// Continues a paused service, optionally waiting for it to enter running state
        /// </summary>
        /// <param name="wait"></param>
        /// <exception cref="ServiceOperationException"></exception>
        public void Continue(bool wait)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                var service = Win32Api.OpenService(
                    scm,
                    _serviceName,
                    ServiceAccessRights.QueryStatus | ServiceAccessRights.PauseContinue
                );
                if (service == IntPtr.Zero)
                {
                    throw new ServiceOperationException(
                        _serviceName,
                        ServiceOperationNames.PAUSE,
                        SERVICE_NOT_INSTALLED
                    );
                }

                try
                {
                    ContinueService(service, wait);
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

        private void StartService(IntPtr service, bool wait)
        {
            if (GetServiceStatus(service) == ServiceState.Running)
            {
                return;
            }

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
            bool wait,
            bool forceIfNecessary)
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
                WaitForServiceToStop(service, process, forceIfNecessary);
            }
            else if (forceIfNecessary)
            {
                SafeKill(process);
            }
        }

        private void SafeKill(Process process)
        {
            if (process.HasExited)
            {
                return;
            }
            var pid = process.Id;

            try
            {
                process.Kill();
            }
            catch
            {
                /* intentionally left blank: perhaps process has exited by itself */
            }
            finally
            {
                if (!process.HasExited)
                {
                    try
                    {
                        // perhaps the OS is taking a little time to get there...
                        process.WaitForExit(1000);
                    }
                    catch
                    {
                        // intentionally suppressed (possible race condition for exit)
                    }

                    if (!process.HasExited)
                    {
                        // ok, give up
                        throw new InvalidOperationException(
                            $"Unable to kill process {pid}"
                        );
                    }
                }
            }
        }

        private void WaitForServiceToStop(
            IntPtr service,
            Process process,
            bool forceIfNecessary)
        {
            var changedStatus = WaitForServiceStatus(
                service,
                ServiceState.StopPending,
                ServiceState.Stopped
            );
            if (!changedStatus &&
                // service is stuck in StopPending & caller has authorised force...
                !forceIfNecessary)
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

                if (forceIfNecessary)
                {
                    waitLevel++;
                    SafeKill(process);
                }
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

        private void PauseService(
            IntPtr service,
            bool wait)
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

        public void ContinueService(
            IntPtr service,
            bool wait)
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

        private bool WaitForServiceStatus(
            IntPtr service,
            ServiceState waitStatus,
            ServiceState desiredStatus
        )
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

            if (ServiceStateExtraWaitSeconds <= 0)
            {
                return status.CurrentState == desiredStatus;
            }

            var waited = 0;
            while (
                waited < ServiceStateExtraWaitSeconds &&
                status.CurrentState != desiredStatus
            )
            {
                Thread.Sleep(1000);
                waited++;
                if (Win32Api.QueryServiceStatus(service, status) == 0)
                {
                    break;
                }
            }

            return status.CurrentState == desiredStatus;
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
            var waitTime = status.WaitHint / 10;

            if (waitTime < 1000) waitTime = 2000;
            if (waitTime > 10000) waitTime = 10000;
            return waitTime;
        }

        private int TryGetServicePid()
        {
            try
            {
                return ServicePID;
            }
            catch
            {
                return 0;
            }
        }

        public KillServiceResult KillService()
        {
            var toKill = TryGetServicePid();
            if (toKill == 0)
            {
                return KillServiceResult.NotRunning;
            }

            var process = Process.GetProcessById(toKill);
            try
            {
                SafeKill(process);
                return process.HasExited
                    ? KillServiceResult.Killed
                    : KillServiceResult.UnableToKill;
            }
            catch
            {
                return KillServiceResult.UnableToKill;
            }
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