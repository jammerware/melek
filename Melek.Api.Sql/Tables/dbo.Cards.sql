IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cards') BEGIN;
	DROP TABLE Cards;
END;
GO

CREATE TABLE [dbo].[Cards]
(
	[CardId] INT NOT NULL PRIMARY KEY,
	[Name] varchar(500) NOT NULL,
	[Cost] varchar(50),
	[Text] varchar(1000),
	[Tribe] varchar(100)
)
GO