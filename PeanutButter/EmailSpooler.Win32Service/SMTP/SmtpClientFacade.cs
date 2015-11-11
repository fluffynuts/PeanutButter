using System.Net.Mail;

namespace EmailSpooler.Win32Service.SMTP
{
    public class SmtpClientFacade: SmtpClient
    {
        public new virtual void Send(MailMessage message)
        {
        }
    }
}