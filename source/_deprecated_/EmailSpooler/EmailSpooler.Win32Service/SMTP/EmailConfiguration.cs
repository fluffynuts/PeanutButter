using System.Configuration;
// ReSharper disable InconsistentNaming

namespace EmailSpooler.Win32Service.SMTP
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
        public string Host { get; }
        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }
        public bool SSLEnabled { get; }
        public EmailConfiguration(string host, int port, string userName, string password, bool sslEnabled)
        {
            Host = host;
            Port = port;
            UserName = userName;
            Password = password;
            SSLEnabled = sslEnabled;
        }

        public static EmailConfiguration CreateFromAppConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;
            return new EmailConfiguration(
                    appSettings["SMTPHost"],
                    int.Parse(appSettings["SMTPPort"]),
                    appSettings["SMTPUserName"],
                    appSettings["SMTPPassword"],
                    bool.Parse(appSettings["SMTPSSL"])
                );
        }
    }
}