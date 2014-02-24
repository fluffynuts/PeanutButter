The Email Spooler Service requires a data store to back it. You can (technically) use
any database which Entity can connect to though this service has only been tested against
MSSQL. This project provides FluentMigrator clasess you can inherit from in your FluentMigrations
project for creating the required tables:
* Email
* EmailRecipient
* EmailAttachment
Conversely, you can run in the raw MSSQL scripts provided (or port to your database and run in there)