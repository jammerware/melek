IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Printings') BEGIN;
	DROP TABLE Printings;
END;
GO

CREATE TABLE [dbo].[Printings]
(
	[PrintingId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[CardId] INT NOT NULL,
	[MultiverseId] varchar(50) NOT NULL,
	[RarityId] INT NOT NULL,
	[SetId] INT NOT NULL,
	[ArtistId] INT NOT NULL,
	[TransformsToMultiverseID] VARCHAR(50), 
	CONSTRAINT [FK_Printings_ToArtists] FOREIGN KEY ([ArtistId]) REFERENCES [Artists]([ArtistId]),
    CONSTRAINT [FK_Printings_ToCards] FOREIGN KEY (CardId) REFERENCES [Cards]([CardId]), 
    CONSTRAINT [FK_Printings_ToSets] FOREIGN KEY ([SetId]) REFERENCES [Sets]([SetId])
);
GO