using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSpooler.Win32Service.DB
{
    public class DataConstants
    {
        public class FieldSizes
        {
            public const int MAX_PATH = 260;
        }
        public class Tables
        {
            public abstract class _Common_
            {
                public const string CREATED = "Created";
                public const string LASTMODIFIED = "LastModified";
                public const string ENABLED = "Enabled";
            }
            public class Email
            {
                public const string NAME = "Email";
                public class Columns : _Common_
                {
                    public const string EMAILID = "EmailID";
                    public const string SENDER = "Sender";
                    public const string SUBJECT = "Subject";
                    public const string BODY = "Body";
                    public const string SENDAT = "SendAt";
                    public const string SENDATTEMPTS = "SendAttempts";
                    public const string SENT = "Sent";
                    public const string LASTERROR = "LastError";
                }
            }
            public class EmailRecipient
            {
                public const string NAME = "EmailRecipient";
                public class Columns : _Common_
                {
                    public const string EMAILRECIPIENTID = "EmailRecipientID";
                    public const string EMAILID = "EmailID";
                    public const string RECIPIENT = "Recipient";
                    public const string PRIMARYRECIPIENT = "PrimaryRecipient";
                    public const string CC = "CC";
                    public const string BCC = "BCC";
                }
            }
            public class EmailAttachment
            {
                public const string NAME = "EmailAttachment";
                public class Columns : _Common_
                {
                    public const string EMAILATTACHMENTID = "EmailAttachmentID";
                    public const string EMAILID = "EmailID";
                    public const string NAME = "Name";
                    public const string INLINE = "Inline";
                    public const string CONTENTID = "ContentID";
                    public const string MIMETYPE = "MIMEType";
                    public const string DATA = "Data";
                }
            }
        }
    }
}
