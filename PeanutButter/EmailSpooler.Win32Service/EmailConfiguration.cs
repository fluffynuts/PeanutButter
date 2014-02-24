using System;
using System.Configuration;
using ServiceShell;

namespace EmailSpooler.Win32Service
{
    public interface IEmailConfiguration
    {
        string Host { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        bool SSLEnabled { get; }
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public bool SSLEnabled { get; private set; }
        public EmailConfiguration(string host, int port, string userName, string password, bool sslEnabled)
        {
            this.Host = host;
            this.Port = port;
            this.UserName = userName;
            this.Password = password;
            this.SSLEnabled = sslEnabled;
        }

        public static EmailConfiguration CreateFromAppConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;
            return new EmailConfiguration(
                    appSettings["SMTPHost"],
                    Int32.Parse(appSettings["SMTPPort"]),
                    appSettings["SMTPUserName"],
                    appSettings["SMTPPassword"],
                    Boolean.Parse(appSettings["SMTPSSL"])
                );
        }
    }
}