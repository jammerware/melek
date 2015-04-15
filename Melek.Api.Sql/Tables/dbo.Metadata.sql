IF OBJECT_ID('dbo.Metadata', 'U') IS NOT NULL
  DROP TABLE dbo.Metadata
GO

CREATE TABLE dbo.Metadata
(
	MetadataID int NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[Version] varchar(20) NOT NULL,
	[DateReleased] datetime NOT NULL DEFAULT(GETDATE())
);
GO