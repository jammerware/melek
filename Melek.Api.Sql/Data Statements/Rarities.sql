IF NOT EXISTS(SELECT * FROM [Rarities] WHERE Name = 'Common') BEGIN;
	INSERT INTO [Rarities](RarityId, Name)
	SELECT 1, 'Common';
END;

IF NOT EXISTS(SELECT * FROM [Rarities] WHERE Name = 'Uncommon') BEGIN;
	INSERT INTO [Rarities](RarityId, Name)
	SELECT 2, 'Uncommon';
END;

IF NOT EXISTS(SELECT * FROM [Rarities] WHERE Name = 'Rare') BEGIN;
	INSERT INTO [Rarities](RarityId, Name)
	SELECT 3, 'Rare';
END;

IF NOT EXISTS(SELECT * FROM [Rarities] WHERE Name = 'Mythic Rare') BEGIN;
	INSERT INTO [Rarities](RarityId, Name)
	SELECT 4, 'Mythic Rare';
END;