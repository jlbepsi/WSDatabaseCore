create database ServiceEPSI_SQLServer
go

use ServiceEPSI_SQLServer
go


-- ************************************** PROCEDURES STOCKEES *******************************************
--
-- Insertion des utilisateurs
--

CREATE PROCEDURE [dbo].[DatabaseAddContributor] (@dbName VARCHAR(30), @login VARCHAR(30), @userRights VARCHAR(300))
AS
BEGIN
	DECLARE @sqlStatement nvarchar(400)
	DECLARE @spot SMALLINT, @aRole VARCHAR(50)

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		SET @sqlStatement = 'USE [' + @dbName + ']; CREATE USER [' + @login + '] FOR LOGIN [' + @login + '];'
		EXEC sp_executesql @sqlStatement

		
		SET @sqlStatement = 'USE [' + @dbName + ']; GRANT ' + @userRights + ' TO [' + @login + '];'
		EXEC(@sqlStatement) 
			
		SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
		EXEC sp_executesql @sqlStatement
	END
END
go


-- **************************************
--
-- Modification des utilisateurs
-- 

CREATE PROCEDURE [dbo].[DatabaseAddOrUpdateContributorGroupType] (@dbName VARCHAR(30), @login VARCHAR(30), @userRights VARCHAR(300), @doUpdate INT)
AS
BEGIN
	DECLARE @sqlStatement nvarchar(400)
	DECLARE @spot SMALLINT, @aRole VARCHAR(50)

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		-- Supression des droits
		IF (@doUpdate = 1)
		BEGIN
			SET @sqlStatement = 'USE [' + @dbName + ']; REVOKE ALTER, DELETE, EXECUTE, INSERT, SELECT, UPDATE, VIEW DEFINITION TO [' + @login + ']'
			EXEC(@sqlStatement) 
		END
			
		-- Ajout des droits	
		SET @sqlStatement = 'USE [' + @dbName + ']; GRANT ' + @userRights + ' TO [' + @login + ']'
		EXEC(@sqlStatement) 
			
		SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
		EXEC sp_executesql @sqlStatement
	END
END
go


CREATE PROCEDURE [dbo].[DatabaseAddOrUpdateUser] (@login nvarchar(50), @password nvarchar(50))
AS
BEGIN
	DECLARE @SqlStatement nvarchar(4000)

	IF EXISTS(SELECT loginname, dbname FROM master.dbo.syslogins WHERE name = @login)
	BEGIN
		SET @sqlStatement = 'USE Master; ALTER LOGIN "' + @login + '" WITH PASSWORD = ''' + @password + ''''
		EXEC(@sqlStatement)
	END
	ELSE
	BEGIN
		SET @SqlStatement = 'CREATE LOGIN [' + @login + '] WITH PASSWORD = ''' + @password + ''''
		EXEC sp_executesql @SqlStatement
	END	
END
go



-- **************************************
--
-- Base de données
-- 

CREATE PROCEDURE [dbo].[DatabaseCreateDatabase] 
@dbName nvarchar(50),
@login nvarchar(50)
AS
BEGIN
	DECLARE @SqlStatement nvarchar(400)
	DECLARE @dataDirectory nvarchar(200)

	IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		SET @SqlStatement = 'USE [master]; CREATE DATABASE [' + @dbName + ']';
		EXEC sp_executesql @SqlStatement
		
		-- Fixe le proprietaire et donc les droits
		SET @sqlStatement = 'ALTER AUTHORIZATION ON DATABASE::[' + @dbName + '] TO [' + @login + ']';
		EXEC sp_executesql @sqlStatement
		
		SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
		EXEC sp_executesql @sqlStatement
	END
END
go




-- **************************************
--
-- Obtention des informations
-- 

CREATE PROCEDURE [dbo].[DatabaseExistsDB] (@dbName VARCHAR(30), @exists INT OUTPUT)
AS
BEGIN
	DECLARE @sqlStatement nvarchar(400)
	DECLARE @spot SMALLINT, @aRole VARCHAR(50)

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		SET @exists = 1
	END
	ELSE
	BEGIN
		SET @exists = 0
	END
END
go


CREATE PROCEDURE [dbo].[DatabaseExistsUser] (@login VARCHAR(30), @exists INT OUTPUT)
AS
BEGIN
	IF EXISTS(SELECT name FROM sys.syslogins WHERE name = @login)
	BEGIN
		SET @exists = 1
	END
	ELSE
	BEGIN
		SET @exists = 0
	END
END
go


CREATE PROCEDURE [dbo].[DatabaseExistsUserInDB]
	@dbName VARCHAR(30), 
	@login VARCHAR(30),
	@exists INT OUTPUT
AS
BEGIN
	DECLARE @sqlStatement nvarchar(400)
	DECLARE @ParmDefinition NVARCHAR(500)

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SET @sqlStatement = 'USE [' + @dbName + ']; SELECT @userCountOUT=COUNT(*) FROM sys.database_principals WHERE name = ''' + @login + ''''

	SET @ParmDefinition = '@userCountOUT varchar(30) OUTPUT'
	EXEC sp_executesql 
		@sqlStatement,
		@ParmDefinition,
		@userCountOUT=@exists OUTPUT
		
	SET @sqlStatement = 'USE master;'
	EXEC sp_executesql @sqlStatement
END
go


CREATE PROCEDURE [dbo].[DatabaseListDatabaseUsers] (@dbName nvarchar(50))
AS
BEGIN
	DECLARE @SqlStatement nvarchar(4000)

	SET @SqlStatement = 'SELECT allUsers.loginname  FROM ' +
		@dbName + '.sys.database_principals AS localUsers, master.sys.syslogins AS allUsers '+
		'WHERE localUsers.sid = allUsers.sid ' +
		'AND localUsers.type IN (''S'', ''U'') '+
		'AND localUsers.sid IS NOT NULL'

	EXEC sp_executesql @SqlStatement
END
go


CREATE PROCEDURE [dbo].[DatabaseListDatabases] (@login VARCHAR(30))
AS
BEGIN
	SELECT TDB.name AS dbName
	FROM sys.databases AS TDB, sys.syslogins AS TUser
	WHERE TDB.owner_sid = TUser.sid
	AND TUser.name = @login
	ORDER BY dbName
END
go


CREATE PROCEDURE [dbo].[DatabaseRemoveContributor] 
	@dbName VARCHAR(30), 
	@login VARCHAR(30)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sqlStatement nvarchar(400)
	DECLARE @spot SMALLINT, @aRole VARCHAR(50)

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		SET @sqlStatement = 'USE [' + @dbName + ']; DROP USER "' + @login + '"'
		EXEC(@sqlStatement) 
	END
END
go


CREATE PROCEDURE [dbo].[DatabaseRemoveDatabase]
	@dbName nvarchar(50)
AS
BEGIN
	DECLARE @SqlStatement nvarchar(400)
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @sqlStatement = 'USE [master]; DROP DATABASE [' + @dbName + ']'
	EXEC sp_executesql @SqlStatement
	
	SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
	EXEC sp_executesql @sqlStatement
END
go


-- **************************************
--
-- Suppression des utilisateurs
-- 

CREATE PROCEDURE [dbo].[DatabaseRemoveUser] (@login nvarchar(50))
AS
BEGIN
	DECLARE @SqlStatement nvarchar(4000)

	IF EXISTS(SELECT loginname, dbname FROM master.dbo.syslogins WHERE name = @login)
	BEGIN
		SET @SqlStatement = 'DROP LOGIN [' + @login + '] '
		EXEC sp_executesql @SqlStatement
	END	
END
go


CREATE PROCEDURE [dbo].[DatabaseAddUser] (@login nvarchar(50), @password nvarchar(50))
AS
BEGIN
	DECLARE @SqlStatement nvarchar(4000)

	IF NOT EXISTS(SELECT loginname, dbname FROM master.dbo.syslogins WHERE name = @login)
	BEGIN
		SET @SqlStatement = 'CREATE LOGIN [' + @login + '] WITH PASSWORD = ''' + @password + ''''
		EXEC sp_executesql @SqlStatement
	END	
END
go


CREATE PROCEDURE [dbo].[DatabaseDeleteDatabase]
	@dbName nvarchar(50)
AS
BEGIN
	DECLARE @SqlStatement nvarchar(400)
	SET NOCOUNT ON;

	SET @sqlStatement = 'USE [master]; DROP DATABASE [' + @dbName + ']'
	EXEC sp_executesql @SqlStatement
	
	SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
	EXEC sp_executesql @sqlStatement
END
go


CREATE PROCEDURE [dbo].[DatabaseUpdateContributorGroupType] (@dbName VARCHAR(30), @login VARCHAR(30), @userRights VARCHAR(300))
AS
BEGIN
	DECLARE @sqlStatement nvarchar(400)
	DECLARE @spot SMALLINT, @aRole VARCHAR(50)

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		-- Supression des droits
		SET @sqlStatement = 'USE [' + @dbName + ']; REVOKE ALTER, DELETE, EXECUTE, INSERT, SELECT, UPDATE, VIEW DEFINITION TO [' + @login + ']'
		EXEC(@sqlStatement) 	
		-- Ajout des droits	
		SET @sqlStatement = 'USE [' + @dbName + ']; GRANT ' + @userRights + ' TO [' + @login + ']'
		EXEC(@sqlStatement) 
			
		SET @sqlStatement = 'USE [ServiceEPSI_SQLServer];'
		EXEC sp_executesql @sqlStatement
	END
END
go


CREATE PROCEDURE [dbo].[DatabaseUpdateContributorPassword] 
	@dbName VARCHAR(30), 
	@login VARCHAR(30), 
	@password VARCHAR(100),
	@userUpdated INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sqlStatement nvarchar(400)
	SET @userUpdated = 0

	IF EXISTS(SELECT name FROM sys.databases WHERE name = @dbName)
	BEGIN
		SET @sqlStatement = 'USE [' + @dbName + ']; ALTER LOGIN "' + @login + '" WITH PASSWORD = ''' + @password + ''''
		EXEC(@sqlStatement)
		SET @userUpdated = 1
	END
END
go


