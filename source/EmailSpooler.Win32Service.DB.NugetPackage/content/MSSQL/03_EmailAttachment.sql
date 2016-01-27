/****** Object:  Table [dbo].[EmailAttachment]    Script Date: 2014-02-24 20:38:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EmailAttachment](
	[EmailAttachmentID] [uniqueidentifier] NOT NULL,
	[EmailID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](260) NOT NULL,
	[Inline] [bit] NOT NULL,
	[ContentID] [nvarchar](260) NOT NULL,
	[MIMEType] [nvarchar](260) NOT NULL,
	[Data] [varbinary](max) NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL,
 CONSTRAINT [PK_EmailAttachment] PRIMARY KEY CLUSTERED 
(
	[EmailAttachmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING ON
GO

ALTER TABLE [dbo].[EmailAttachment] ADD  CONSTRAINT [DF_EmailAttachment_Inline]  DEFAULT ((0)) FOR [Inline]
GO

ALTER TABLE [dbo].[EmailAttachment] ADD  CONSTRAINT [DF_EmailAttachment_ContentID]  DEFAULT (newid()) FOR [ContentID]
GO

ALTER TABLE [dbo].[EmailAttachment] ADD  CONSTRAINT [DF_EmailAttachment_MIMEType]  DEFAULT ('application/octet-stream') FOR [MIMEType]
GO

ALTER TABLE [dbo].[EmailAttachment] ADD  CONSTRAINT [DF_EmailAttachment_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[EmailAttachment] ADD  CONSTRAINT [DF_EmailAttachment_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO

ALTER TABLE [dbo].[EmailAttachment]  WITH CHECK ADD  CONSTRAINT [FK_EmailAttachment_EmailID_Email_EmailID] FOREIGN KEY([EmailID])
REFERENCES [dbo].[Email] ([EmailID])
GO

ALTER TABLE [dbo].[EmailAttachment] CHECK CONSTRAINT [FK_EmailAttachment_EmailID_Email_EmailID]
GO


