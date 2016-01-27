This is a generic email spooling service to run on win32 hosts. It uses a backing store
in (technically) any database Entity can connect to, though it's only been tested on
MSSQL. FluentMigrator classes and raw SQL scripts for creating the necessary tables
are included in EmailSPooler.Win32Service.DB.

The basic idea is to move the actual act of email sending, via SMTP, into the domain
of a service dedicated to the process so that you can just run tests in your application's
test suite against the *registration* of emails instead of the sending of emails.

In addition, the process allows for handling of attachments and inline-attachments. When 
you specify the ContentID of an email attachment, you may use that content ID to reference
the attachment in your mail, for example, when referencing an attached image in an HTML
mail, you would link with:
<img src="cid:{content id}"/>
where {content id} is the value of the ContentID field. By default, this will be generated
as a guid, if not specified, so you can choose the order of operations which suits you best:
1) create an attachment entry without setting the content ID, read back the content ID and
   use it for links in HTML content you create to put into the Email table
or
2) Specify the value for ContentID (note that it has a max-length of 260 characters, chosen
   because it's the safe limit for path name lengths, register the attachment with your
   id of choice and use that id in your email body. I'd recommend sticking to using a guid
   though -- it's just easier and safer

Once you've set up the database schema objects required for this service and configured
the EmailSPooler.Win32Service.exe.config with a connection string to your database, you
can install the service with the commandline switch '-i' or uninstall with '-u'. Use '-h'
for help. You can also do a "one-time" run from the console with '-r' -- which is useful
when attempting to diagnose and fix issues with a new installation where, perhaps the
database hasn't been set up properly yet or login credentials don't work, etc.

Also, don't forget to configure the SMTP details:
* SMTPHost (dns name or ip address)
* SMTPPort (eg 25 for regular STMP or 587 for SSL-enabled servers (generally))
* SMTPUserName
* SMTPPassword
* SMTPSSL ("True" or "False", used to determine if communications with your SMTP server
  require SSL

You can use this service against a gmail account with your credentials and:
SMTPHost: smtp.gmail.com
SMTPPort: 587
SMTPSSL: True

Note, however, that doing so may violate your terms of use for the service, so don't
hold me responsible if Google acts against you. In reality though, I've used this
mechanism as a "cheap" way to enable email for a low-volume application.

You can use InnoSetup and the provided script in EmailSPooler.Win32Service.Setup to create
a neatly-bundled installer for this service for client-side deployment.