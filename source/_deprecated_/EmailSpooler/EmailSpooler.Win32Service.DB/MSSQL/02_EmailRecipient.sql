/****** Object:  Table [dbo].[EmailRecipient]    Script Date: 2014-02-24 20:38:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmailRecipient](
	[EmailRecipientID] [uniqueidentifier] NOT NULL,
	[EmailID] [uniqueidentifier] NOT NULL,
	[Recipient] [nvarchar](max) NOT NULL,
	[PrimaryRecipient] [bit] NOT NULL,
	[CC] [bit] NOT NULL,
	[BCC] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL,
 CONSTRAINT [PK_EmailRecipient] PRIMARY KEY CLUSTERED 
(
	[EmailRecipientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[EmailRecipient] ADD  CONSTRAINT [DF_EmailRecipient_PrimaryRecipient]  DEFAULT ((1)) FOR [PrimaryRecipient]
GO

ALTER TABLE [dbo].[EmailRecipient] ADD  CONSTRAINT [DF_EmailRecipient_CC]  DEFAULT ((0)) FOR [CC]
GO

ALTER TABLE [dbo].[EmailRecipient] ADD  CONSTRAINT [DF_EmailRecipient_BCC]  DEFAULT ((0)) FOR [BCC]
GO

ALTER TABLE [dbo].[EmailRecipient] ADD  CONSTRAINT [DF_EmailRecipient_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[EmailRecipient] ADD  CONSTRAINT [DF_EmailRecipient_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO

ALTER TABLE [dbo].[EmailRecipient]  WITH CHECK ADD  CONSTRAINT [FK_EmailRecipient_EmailID_Email_EmailID] FOREIGN KEY([EmailID])
REFERENCES [dbo].[Email] ([EmailID])
GO

ALTER TABLE [dbo].[EmailRecipient] CHECK CONSTRAINT [FK_EmailRecipient_EmailID_Email_EmailID]
GO


