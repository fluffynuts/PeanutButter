using System;
using System.Collections.Specialized;
using System.Configuration;
using PeanutButter.ServiceShell;

namespace EmailSpooler.Win32Service
{
    public class EmailSpoolerConfig : IEmailSpoolerConfig
    {
        private readonly NameValueCollection _appSettings;
        // ReSharper disable once MemberCanBePrivate.Global
        public ISimpleLogger Logger { get; protected set; }
        public int MaxSendAttempts { get; private set; }
        public int BackoffIntervalInMinutes { get; private set; }
        public int BackoffMultiplier { get; private set; }
        public int PurgeMessageWithAgeInDays { get; private set; }
        public EmailSpoolerConfig(ISimpleLogger logger)
        {
            _appSettings = ConfigurationManager.AppSettings;
            Init(logger);
        }

        internal EmailSpoolerConfig(ISimpleLogger logger, NameValueCollection appSettings)
        {
            _appSettings = appSettings;
            Init(logger);
        }

        private void Init(ISimpleLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Logger = logger;
            MaxSendAttempts = GetConfiguredIntVal("MaxSendAttempts", 5);
            BackoffIntervalInMinutes = GetConfiguredIntVal("BackoffIntervalInMinutes", 2);
            BackoffMultiplier = GetConfiguredIntVal("BackoffMultiplier", 2);
            PurgeMessageWithAgeInDays = GetConfiguredIntVal("PurgeMessageWithAgeInDays", 30);
        }

        private int GetConfiguredIntVal(string keyName, int defaultValue)
        {
            var configured = _appSettings[keyName];
            if (configured == null)
            {
                Logger.LogInfo($"No configured value for '{keyName}'; falling back on default value: '{defaultValue}'");
                return defaultValue;
            }
            int configuredValue;
            if (int.TryParse(configured, out configuredValue))
                return configuredValue;
            Logger.LogWarning($"Configured value of '{configured}' cannot be parsed into an integer; falling back on default value '{defaultValue}'");
            return defaultValue;
        }

    }
}