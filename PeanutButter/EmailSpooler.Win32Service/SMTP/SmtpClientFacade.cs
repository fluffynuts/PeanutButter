using System.Net;
using System.Net.Mail;

namespace EmailSpooler.Win32Service.SMTP
{
    public interface ISmtpClient
    {
        string Host { get; set; }
        int Port { get; set; }
        bool EnableSsl { get; set; }
        bool UseDefaultCredentials { get; set; }
        ICredentialsByHost Credentials { get; set; }
        void Send(MailMessage message);
    }

    public class SmtpClientFacade : ISmtpClient
    {
        private SmtpClient _actual;
        public string Host
        {
            get { return _actual.Host; }
            set { _actual.Host = value; }
        }
        public int Port
        {
            get { return _actual.Port; }
            set { _actual.Port = value; }
        }
        public bool EnableSsl
        {
            get { return _actual.EnableSsl; }
            set { _actual.EnableSsl = value; }
        }
        public bool UseDefaultCredentials
        {
            get { return _actual.UseDefaultCredentials; }
            set { _actual.UseDefaultCredentials = value; }
        }
        public ICredentialsByHost Credentials
        {
            get { return _actual.Credentials; }
            set { _actual.Credentials = value; }
        }

        public SmtpClientFacade()
        {
            _actual = new SmtpClient();
        }

        public void Send(MailMessage message)
        {
            _actual.Send(message);
        }
    }
}