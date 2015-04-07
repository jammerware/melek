IF NOT EXISTS(SELECT * FROM [Format] WHERE Name = 'Standard') BEGIN;
	INSERT INTO [Format](FormatId, Name)
	SELECT 1, 'Standard';
END;

IF NOT EXISTS(SELECT * FROM [Format] WHERE Name = 'Modern') BEGIN;
	INSERT INTO [Format](FormatId, Name)
	SELECT 2, 'Modern';
END;

IF NOT EXISTS(SELECT * FROM [Format] WHERE Name = 'Legacy') BEGIN;
	INSERT INTO [Format](FormatId, Name)
	SELECT 3, 'Legacy';
END;

IF NOT EXISTS(SELECT * FROM [Format] WHERE Name = 'Vintage') BEGIN;
	INSERT INTO [Format](FormatId, Name)
	SELECT 4, 'Vintage';
END;

IF NOT EXISTS(SELECT * FROM [Format] WHERE Name = 'Commander') BEGIN;
	INSERT INTO [Format](FormatId, Name)
	SELECT 5, 'Commander';
END;