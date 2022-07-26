/****** Object:  Table [dbo].[COMBlockList]    Script Date: 25 Nov 2015 9:23:57 AM ******/
CREATE TABLE [dbo].[COMBlockList](
                [COMBlockListID] [int] IDENTITY(1,1) NOT NULL,
                [COMBlockListReasonID] [int] NOT NULL,
                [COREClientID] [int] NOT NULL,
                [Source] [nvarchar](250) NOT NULL,
                [SourceBy] [int] NULL,
                [Date] [datetime] NOT NULL,
                [COMBulkID] [int] NULL,
                [CORECustomerID] [int] NULL,
                [COREMemberID] [int] NULL,
                [Email] [nvarchar](200) NULL,
                [Mobile] [nvarchar](20) NULL,
                [DeviceKeyID] [nvarchar](100) NULL,
                [ReasonDescription] [nvarchar](2000) NOT NULL,
                [IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMBlockListID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
 
CREATE TABLE [dbo].[COMBlockListReason](
                [COMBlockListReasonID] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Order] [int] NOT NULL,
                [IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMBlockListReasonID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
CREATE TABLE [dbo].[COMMessagePlatformOption](
                [COMMessagePlatformOptionID] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [DataType] [nvarchar](50) NOT NULL,
                [MaxSize] [int] NOT NULL,
                [IsActive] [bit] NOT NULL,
CONSTRAINT [PK_COMMessagePlatformOption] PRIMARY KEY CLUSTERED 
(
                [COMMessagePlatformOptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
CREATE TABLE [dbo].[COMMessageRequestLog](
                [COMMessageRequestLogID] [int] IDENTITY(1,1) NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [Source] [nvarchar](200) NOT NULL,
                [TimeStamp] [datetime] NOT NULL,
                [COREClientID] [int] NOT NULL,
                [COREMemberID] [int] NOT NULL,
                [Status] [nvarchar](50) NOT NULL,
CONSTRAINT [PK_COMMessageRequestLog] PRIMARY KEY CLUSTERED 
(
                [COMMessageRequestLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMNotificationCustomer]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMNotificationCustomer](
                [COMNotificationCustomerID] [int] IDENTITY(1,1) NOT NULL,
                [COMCustomerID] [int] NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMNotificationCustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMNotificationCustomerHistory]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMNotificationCustomerHistory](
                [COMNotificationCustomerHistoryID] [int] IDENTITY(1,1) NOT NULL,
                [COMNotificationCustomerID] [int] NOT NULL,
                [COREMemberID] [int] NULL,
                [Date] [datetime] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMNotificationCustomerHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMNotificationMember]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMNotificationMember](
                [COMNotificationMemberID] [int] IDENTITY(1,1) NOT NULL,
                [COREMemberID] [int] NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMNotificationMemberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMNotificationMemberHistory]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMNotificationMemberHistory](
                [COMNotificationMemberHistoryID] [int] IDENTITY(1,1) NOT NULL,
                [COMNotificationMemberID] [int] NOT NULL,
                [COREMemberID] [int] NULL,
                [Date] [datetime] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMNotificationMemberHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMNotificationRestriction]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMNotificationRestriction](
                [COMNotificationRestrictionID] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMNotificationRestrictionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMPromotionCustomer]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPromotionCustomer](
                [COMPromotionCustomerID] [int] IDENTITY(1,1) NOT NULL,
                [COMCustomerID] [int] NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMPromotionCustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMPromotionCustomerHistory]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPromotionCustomerHistory](
                [COMPromotionCustomerHistoryID] [int] IDENTITY(1,1) NOT NULL,
                [COMPromotionCustomerID] [int] NOT NULL,
                [COREMemberID] [int] NULL,
                [Date] [datetime] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMPromotionCustomerHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMPromotionMember]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPromotionMember](
                [COMPromotionMemberID] [int] IDENTITY(1,1) NOT NULL,
                [COREMemberID] [int] NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMPromotionMemberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMPromotionMemberHistory]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPromotionMemberHistory](
                [COMPromotionMemberHistoryID] [int] IDENTITY(1,1) NOT NULL,
                [COMPromotionMemberID] [int] NOT NULL,
                [COREMemberID] [int] NULL,
                [Date] [datetime] NOT NULL,
                [COMSubscriptionOptionID] [int] NOT NULL,
                [COMNotificationRestrictionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMPromotionMemberHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMProtocol]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMProtocol](
                [COMProtocolID] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [AssemblyName] [nvarchar](100) NOT NULL,
                [IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMProtocolID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMProtocolOption]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMProtocolOption](
                [COMProtocolOptionID] [int] IDENTITY(1,1) NOT NULL,
                [COMProtocolID] [int] NOT NULL,
                [COMMessagePlatformOptionID] [int] NOT NULL,
                [IsRequired] [bit] NOT NULL,
CONSTRAINT [PK_COMProtocolOption] PRIMARY KEY CLUSTERED 
(
                [COMProtocolOptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO
/****** Object:  Table [dbo].[COMSubscriptionOption]    Script Date: 25 Nov 2015 9:23:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMSubscriptionOption](
                [COMSubscriptionOptionID] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
                [COMSubscriptionOptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
 
GO


