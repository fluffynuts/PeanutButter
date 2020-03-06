using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// The name of the service, usable from, eg 'net stop {name}'
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// The name displayed in the service manager (service.msc)
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The full commandline run for this service
        /// </summary>
        string Commandline { get; }

        /// <summary>
        /// The executable of the service
        /// </summary>
        string ServiceExe { get; }

        /// <summary>
        /// Any arguments this service is run with
        /// </summary>
        string[] Arguments { get; }

        /// <summary>
        /// The current state of this service
        /// </summary>
        ServiceState State { get; }

        /// <summary>
        /// Whether or not this service is installed; will
        /// also return false if the service is marked for deletion.
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Test if the service is marked for deletion.
        /// </summary>
        bool IsMarkedForDelete { get; }

        /// <summary>
        /// How many extra seconds to wait on service operations,
        /// outside of the recommended wait hint provided by
        /// the ServiceControlManager
        /// </summary>
        int ServiceStateExtraWaitSeconds { get; set; }

        /// <summary>
        /// Startup type for service, eg auto, disabled, manual
        /// </summary>
        ServiceStartupTypes StartupType { get; }

        /// <summary>
        /// The current process id of the service, if running.
        /// Will return -1 if not running
        /// </summary>
        int ServicePID { get; }

        /// <summary>
        /// True if the service is in a stoppable state
        /// </summary>
        bool IsStoppable { get; }

        /// <summary>
        /// True if the service is in a startable state
        /// </summary>
        bool IsStartable { get; }

        /// <summary>
        /// Uninstall the service, waiting for completion and killing
        /// the service process if it doesn't stop in time, as
        /// determined by the SCM wait hint and any extra time specified
        /// in ServiceStateExtraWaitSeconds
        /// </summary>
        void Uninstall();

        [Obsolete("Rather use the overload with ControlOptions")]
        void Uninstall(bool waitForUninstall);

        /// <summary>
        /// Uninstall the service, with full control over waiting and
        /// forceful stopping.
        /// </summary>
        /// <param name="options"></param>
        void Uninstall(ControlOptions options);

        /// <summary>
        /// Installs and starts the service, waiting for the service
        /// to report that it's running
        /// </summary>
        void InstallAndStart();

        /// <summary>
        /// Install and start the service, with control over whether
        /// to wait for the service to have properly started
        /// </summary>
        /// <param name="waitForStart"></param>
        void InstallAndStart(bool waitForStart);

        /// <summary>
        /// Install the service only
        /// </summary>
        void Install();

        /// <summary>
        /// Start the service only, waiting for it to enter the Running state
        /// </summary>
        void Start();

        /// <summary>
        /// Start the service, optionally waiting to enter the Running state
        /// </summary>
        /// <param name="wait"></param>
        void Start(bool wait);

        /// <summary>
        /// Stop the service, waiting for it to come to rest and forcing stop
        /// if it doesn't do so in time, as determined by the SCM wait hint
        /// and any extra time specified in ServiceStateExtraWaitSeconds
        /// </summary>
        void Stop();

        [Obsolete("Rather use the overload with ControlOptions")]
        void Stop(bool wait);

        /// <summary>
        /// Stop the service, with full control over whether to wait and
        /// whether to force stop
        /// </summary>
        /// <param name="options"></param>
        void Stop(ControlOptions options);

        /// <summary>
        /// Pause the service, if running and wait for change of state
        /// </summary>
        void Pause();

        /// <summary>
        /// Pause the service, if running, optionally waiting
        /// </summary>
        void Pause(bool wait);

        /// <summary>
        /// Continue a paused service, waiting for change of state
        /// </summary>
        void Continue();

        /// <summary>
        /// Continue a paused service, optionally waiting for change of state
        /// </summary>
        void Continue(bool wait);

        /// <summary>
        /// Disable the service from running
        /// </summary>
        void Disable();

        /// <summary>
        /// Set the service to automatically start with the host system
        /// </summary>
        void SetAutomaticStart();

        /// <summary>
        /// Set the service to require manual start
        /// </summary>
        void SetManualStart();

        /// <summary>
        /// Kills the service, if running
        /// </summary>
        /// <returns></returns>
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

        /// <inheritdoc />
        public string ServiceName => _serviceName;

        /// <inheritdoc />
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

        /// <inheritdoc />
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
        /// <inheritdoc />
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value ?? _serviceName;
        }

        private string _displayName;
        private readonly string _serviceCommandline;
        private string _serviceExe;

        /// <inheritdoc />
        public string Commandline =>
            _serviceCommandline ?? FindCommandlineForServiceName(ServiceName);

        /// <inheritdoc />
        public string ServiceExe
            => _serviceExe ??= QueryExeForServiceName(ServiceName);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int ServiceStateExtraWaitSeconds { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool IsInstalled =>
            Win32ApiMethodForQueryingServiceInstalled();

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Uninstall()
        {
            Uninstall(true);
        }

        /// <inheritdoc />
        public void Uninstall(bool waitForUninstall)
        {
            Uninstall(
                waitForUninstall
                    ? ControlOptions.Wait
                    : ControlOptions.None
            );
        }

        /// <inheritdoc />
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
        /// <inheritdoc />
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

        /// <inheritdoc />
        public void InstallAndStart()
        {
            InstallAndStart(true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Disable()
        {
            SetStartValue(ServiceStartupTypes.Disabled);
        }

        /// <inheritdoc />
        public void SetAutomaticStart()
        {
            SetStartValue(ServiceStartupTypes.Automatic);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Start()
        {
            Start(true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Stop()
        {
            Stop(true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Pause()
        {
            Pause(true);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Continue()
        {
            Continue(true);
        }

        /// <inheritdoc />
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
            if (!wait)
            {
                return;
            }

            if (WaitForServiceStatus(
                service,
                ServiceState.StartPending,
                ServiceState.Running))
            {
                return;
            }

            throw new ServiceOperationException(_serviceName,
                ServiceOperationNames.START,
                "Unable to start service");
        }

        private void StopService(
            IntPtr service,
            bool wait,
            bool forceIfNecessary)
        {
            if (GetServiceStatus(service) == ServiceState.Stopped)
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
                if (process.WaitForExit(ServiceStateExtraWaitSeconds * 1000))
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

            var waitUntil = DateTime.Now.AddSeconds(ServiceStateExtraWaitSeconds);
            while (
                DateTime.Now > waitUntil &&
                status.CurrentState != desiredStatus
            )
            {
                Thread.Sleep(200);
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

        /// <inheritdoc />
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