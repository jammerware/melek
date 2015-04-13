﻿IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rulings') BEGIN;
	DROP TABLE Rulings;
END;
GO

CREATE TABLE [dbo].[Rulings]
(
	[RulingId] INT NOT NULL PRIMARY KEY,
	[CardId] INT NOT NULL, 
	[Date] DATETIME,
	[Text] varchar(8000) NOT NULL, 
    CONSTRAINT [FK_Rulings_ToCards] FOREIGN KEY ([CardId]) REFERENCES [Cards]([CardId])
);
GO