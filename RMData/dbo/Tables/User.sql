CREATE TABLE [dbo].[User]
(
    [UserId] NVARCHAR(128) NOT NULL PRIMARY KEY, 
    [FirstName] NVARCHAR(50) NOT NULL, 
    [LastName] NVARCHAR(50) NOT NULL, 
    [Email] NVARCHAR(256) NOT NULL, 
    [CreatDate] DATETIME2 NOT NULL DEFAULT getutcdate()
)
