USE master;
GO

IF DB_ID('municipalite') IS NULL
	CREATE DATABASE municipalite;
GO

USE municipalite;
GO

-- DROP TABLE municipalite;
IF NOT EXISTS (SELECT * FROM sysobjects WHERE [name]='municipalite' AND xtype='U')
	CREATE TABLE municipalite(
		municipaliteId INT PRIMARY KEY,
		nomMunicipalite VARCHAR(150) NOT NULL,
		adresseCourriel VARCHAR(150) NOT NULL,
		adresseWeb VARCHAR(1024) NOT NULL,
		dateProchaineElection DATETIME2 NULL,
		actif BIT
	);
