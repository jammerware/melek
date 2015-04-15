IF NOT EXISTS(SELECT * FROM Metadata WHERE [Version] = '1.0.0') BEGIN;
	INSERT INTO MetaData (
		[Version],
		DateReleased
	)
	SELECT 
		'1.0.0',
		GETDATE();
END;