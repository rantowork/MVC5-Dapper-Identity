SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users] (
    [Id]                NVARCHAR (100) NOT NULL,
    [UserName]          NVARCHAR (MAX) NOT NULL,
    [Nickname]          NVARCHAR (MAX) NOT NULL,
    [PasswordHash]      NVARCHAR (MAX) NULL,
    [SecurityStamp]     NVARCHAR (MAX) NULL,
    [IsConfirmed]       BIT            NOT NULL,
    [ConfirmationToken] NVARCHAR (MAX) NULL,
    [CreatedDate]       DATETIME       NOT NULL
);