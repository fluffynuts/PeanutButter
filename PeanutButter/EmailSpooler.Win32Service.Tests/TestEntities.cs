using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Validation;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.Models;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace EmailSpooler.Win32Service.Tests
{
    [TestFixture]
    public class TestEntities
    {
        [Test]
        public void TestAgainstDatabase()
        {
            //---------------Set up test pack-------------------
            using (var db = new TempDB())
            {
                var ctx = new EmailContext(db.CreateConnection());
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var email = new Models.Email 
                {
                    Subject = RandomValueGen.GetRandomString(), 
                    Sender = RandomValueGen.GetRandomString(),
                    Body = RandomValueGen.GetRandomString(),
                    SendAt = DateTime.Now
                };

                var recipient = new Models.EmailRecipient 
                {
                    Recipient = "foo@bar.com"
                };
                email.EmailRecipients.Add(recipient);

                var attachment = new Models.EmailAttachment 
                {
                    Data = RandomValueGen.GetRandomBytes(), 
                    MIMEType = "application/octet-stream", 
                    Name = "sneaky.file",
                    ContentID = Guid.NewGuid().ToString()
                };

                email.EmailAttachments.Add(attachment);

                ctx.Emails.Add(email);

                try
                {
                    ctx.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    var message = String.Join("\n\n", ex.EntityValidationErrors.Select(e => 
                        String.Join("\n", e.ValidationErrors.Select(ve => ve.ErrorMessage))));
                    throw new Exception(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }

                CollectionAssert.IsNotEmpty(ctx.Emails);

                var ctx2 = new EmailContext(db.CreateConnection());
                var email2 = ctx.Emails.FirstOrDefault();
                Assert.IsNotNull(email2);
                PropertyAssert.AllPropertiesAreEqual(email, email2);
                //---------------Test Result -----------------------
            }
        }
    }

    public class TempDB : IDisposable
    {
        public string DatabaseFile { get; private set; }
        public string ConnectionString { get; private set; }
        private static Semaphore _lock = new Semaphore(1, 1);
        private List<DbConnection> _connections;

        public TempDB()
        {
            using (new AutoLocker(_lock))
            {
                DatabaseFile = Path.GetTempFileName();
                if (File.Exists(DatabaseFile))
                    File.Delete(DatabaseFile);
                ConnectionString = String.Format("DataSource=\"{0}\";Password=\"{1}\"", DatabaseFile, "password");
                using (var engine = new SqlCeEngine(ConnectionString))
                {
                    engine.CreateDatabase();
                }
            }
            this.CreateEmailTables();
            this._connections = new List<DbConnection>();
        }

        public DbConnection CreateConnection()
        {
            var sqlCeConnection = new SqlCeConnection(this.ConnectionString);
            this._connections.Add(sqlCeConnection);
            return sqlCeConnection;
        }

        private void CreateEmailTables()
        {
            const string EMAIL_TABLE = @"
CREATE TABLE [Email] (
	[EmailID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Sender] [nvarchar](4000) NULL,
	[Subject] [nvarchar](4000) NOT NULL,
	[Body] [nvarchar](4000) NOT NULL,
	[SendAt] [datetime] NOT NULL,
	[SendAttempts] [int] NOT NULL,
	[Sent] [bit] NOT NULL,
	[LastError] [nvarchar](4000) NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL
)";
            const string EMAIL_RECIPIENT_TABLE = @"
CREATE TABLE [EmailRecipient] (
	[EmailRecipientID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[EmailID] [uniqueidentifier] NOT NULL,
	[Recipient] [nvarchar](4000) NOT NULL,
	[PrimaryRecipient] [bit] NOT NULL,
	[CC] [bit] NOT NULL,
	[BCC] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL);";
            const string EMAIL_ATTACHMENT_TABLE = @"
CREATE TABLE [EmailAttachment](
	[EmailAttachmentID] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[EmailID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](260) NOT NULL,
	[Inline] [bit] NOT NULL,
	[ContentID] [nvarchar](260) NOT NULL,
	[MIMEType] [nvarchar](260) NOT NULL,
	[Data] [image] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL)";
            using (var disposer = new AutoDisposer())
            {
                var conn = disposer.Add(new SqlCeConnection(this.ConnectionString));
                conn.Open();
                var cmd = disposer.Add(conn.CreateCommand());
                foreach (var script in new[] { EMAIL_TABLE, EMAIL_RECIPIENT_TABLE, EMAIL_ATTACHMENT_TABLE })
                {
                    cmd.CommandText = script;
                    Action<string> debug = s => System.Diagnostics.Debug.WriteLine(s);
                    debug("Running script:");
                    debug(script);
                    debug("---------");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            foreach (var conn in this._connections)
            {
                try
                {
                    conn.Dispose();
                }
                catch { }
            }
            this._connections.Clear();
            try
            {
                File.Delete(DatabaseFile);
            }
            catch
            {
            }
        }
    }
}
