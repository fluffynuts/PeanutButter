using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace EmailSpooler.Win32Service.SMTP
{
    public interface IEmail : IDisposable
    {
        string From { get; set; }
        List<string> To { get; }
        List<string> CC { get; }
        List<string> BCC { get; }
        string Subject { get; set; }
        string Body { get; set; }
        List<EmailAttachment> Attachments { get; }
        void Send();
        string AddPDFAttachment(string fileName, byte[] data);
        string AddAttachment(string fileName, byte[] data, string mimeType, bool isInline = false);
        void AddAttachment(string fileName, byte[] data, string mimeType, string contentId);
        string AddInlineImageAttachment(string fileName, byte[] data);
        void AddRecipient(string email);
        void AddCC(string email);
        void AddBCC(string email);
    }
}