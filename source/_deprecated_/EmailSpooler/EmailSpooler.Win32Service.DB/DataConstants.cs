// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable ClassNeverInstantiated.Global
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
            public abstract class CommonColumns
            {
                public const string CREATED = "Created";
                public const string LASTMODIFIED = "LastModified";
                public const string ENABLED = "Enabled";
            }
            public class Emails
            {
                public const string NAME = "Emails";
                public class Columns : CommonColumns
                {
                    public const string EMAILID = "EmailId";
                    public const string SENDER = "Sender";
                    public const string SUBJECT = "Subject";
                    public const string BODY = "Body";
                    public const string SENDAT = "SendAt";
                    public const string SENDATTEMPTS = "SendAttempts";
                    public const string SENT = "Sent";
                    public const string LASTERROR = "LastError";
                }
            }
            public class EmailRecipients
            {
                public const string NAME = "EmailRecipients";
                public class Columns : CommonColumns
                {
                    public const string EMAILRECIPIENTID = "EmailRecipientId";
                    public const string EMAILID = "EmailId";
                    public const string RECIPIENT = "Recipient";
                    public const string IS_PRIMARYRECIPIENT = "IsPrimaryRecipient";
                    public const string IS_CC = "IsCC";
                    public const string IS_BCC = "IsBCC";
                }
            }
            public class EmailAttachments
            {
                public const string NAME = "EmailAttachments";
                public class Columns : CommonColumns
                {
                    public const string EMAILATTACHMENTID = "EmailAttachmentId";
                    public const string EMAILID = "EmailId";
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
