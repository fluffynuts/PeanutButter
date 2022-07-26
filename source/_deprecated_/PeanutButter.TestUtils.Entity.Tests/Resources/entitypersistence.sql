create table [SomeEntityWithDecimalValue] (
    SomeEntityWithDecimalValueId int identity primary key,
    DecimalValue decimal not null,
    Created datetime not null default CURRENT_TIMESTAMP,
    Enabled bit not null default 1,
    LastUpdated datetime null
);
GO

