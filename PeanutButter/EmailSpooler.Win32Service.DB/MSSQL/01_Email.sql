USE [EACH]
GO

/****** Object:  Table [dbo].[Email]    Script Date: 2014-02-24 20:38:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Email](
	[EmailID] [bigint] IDENTITY(1,1) NOT NULL,
	[Sender] [nvarchar](max) NULL,
	[Subject] [nvarchar](max) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[SendAt] [datetime] NOT NULL,
	[SendAttempts] [int] NOT NULL,
	[Sent] [bit] NOT NULL,
	[LastError] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[LastModified] [datetime] NULL,
	[Enabled] [bit] NOT NULL,
 CONSTRAINT [PK_Email] PRIMARY KEY CLUSTERED 
(
	[EmailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Email] ADD  CONSTRAINT [DF_Email_SendAt]  DEFAULT (getdate()) FOR [SendAt]
GO

ALTER TABLE [dbo].[Email] ADD  CONSTRAINT [DF_Email_SendAttempts]  DEFAULT ((0)) FOR [SendAttempts]
GO

ALTER TABLE [dbo].[Email] ADD  CONSTRAINT [DF_Email_Sent]  DEFAULT ((0)) FOR [Sent]
GO

ALTER TABLE [dbo].[Email] ADD  CONSTRAINT [DF_Email_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Email] ADD  CONSTRAINT [DF_Email_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO


