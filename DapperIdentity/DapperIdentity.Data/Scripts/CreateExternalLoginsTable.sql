SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ExternalLogins] (
    [ExternalLoginId] UNIQUEIDENTIFIER NOT NULL,
    [UserId]          NVARCHAR (100)   NOT NULL,
    [LoginProvider]   NVARCHAR (MAX)   NOT NULL,
    [ProviderKey]     NVARCHAR (MAX)   NOT NULL
);