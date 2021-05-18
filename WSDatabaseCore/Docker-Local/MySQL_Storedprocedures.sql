
DROP PROCEDURE IF EXISTS AddOrUpdateUser;
DROP PROCEDURE IF EXISTS DropDB;
DROP PROCEDURE IF EXISTS DropUser;
DROP PROCEDURE IF EXISTS ExistDB;
DROP PROCEDURE IF EXISTS ExistsUser;
DROP PROCEDURE IF EXISTS ExistsUserInDB;
DROP PROCEDURE IF EXISTS ListDatabases;
DROP PROCEDURE IF EXISTS UpdateUserPassword;



DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `AddOrUpdateUser`(IN `userName` VARCHAR(30), IN `userPassword` VARCHAR(30))
BEGIN
	DECLARE userCount INTEGER; 
	
	SELECT COUNT(*) INTO userCount  FROM mysql.user WHERE USER = userName;
	IF  userCount = 0 THEN
		SET @SQLStmt = CONCAT("CREATE USER '", userName, "'@'%' IDENTIFIED BY '", userPassword, "';");
	ELSE
	  SET @SQLStmt = CONCAT("ALTER USER '", userName, "' IDENTIFIED BY '", userPassword, "';");
	END IF;
	
	PREPARE Stmt FROM @SQLStmt;
	EXECUTE Stmt;
	DEALLOCATE PREPARE Stmt;
	
	FLUSH PRIVILEGES;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`%` PROCEDURE `DropDB`(IN `dbName` VARCHAR(50))
    NO SQL
BEGIN
    DECLARE SQLStmt TEXT;

  SET @SQLStmt = CONCAT('DROP DATABASE ', dbName);
  PREPARE Stmt FROM @SQLStmt;
  EXECUTE Stmt;
  DEALLOCATE PREPARE Stmt;
  
  
    DELETE FROM mysql.db WHERE Db=dbName;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `DropUser`(IN userName VARCHAR(30))
BEGIN
	DECLARE userCount INTEGER;
	
	SELECT COUNT(*) INTO userCount  FROM mysql.user WHERE USER = userName;
	IF  userCount = 1 THEN
		DELETE FROM user WHERE USER = userName;
		FLUSH PRIVILEGES;
	END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`%` PROCEDURE `ExistDB`(IN `dbName` VARCHAR(50), OUT `dbExists` INT)
    NO SQL
SELECT COUNT(*) INTO dbExists FROM information_schema.schemata WHERE SCHEMA_NAME=dbName$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `ExistsUser`(IN `userName` VARCHAR(30), OUT `userExists` INT)
BEGIN
	SELECT COUNT(*) INTO userExists  FROM mysql.user WHERE USER = userName;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`%` PROCEDURE `ExistsUserInDB`(IN `dbName` VARCHAR(50), IN `userName` VARCHAR(30), OUT `userExists` INT)
    NO SQL
SELECT COUNT(*) INTO userExists FROM mysql.db WHERE User = userName AND Db = dbName$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `ListDatabases`(IN `userName` VARCHAR(30))
BEGIN
	SELECT db FROM mysql.db WHERE user = userName;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`%` PROCEDURE `UpdateUserPassword`( IN userName VARCHAR(30), IN userPassword VARCHAR(30), OUT userUpdated INT)
BEGIN
	SELECT COUNT(*) INTO userUpdated  FROM mysql.user WHERE USER = userName;
	
	IF  userUpdated = 1 THEN
	  	SET @SQLStmt = CONCAT("ALTER USER ", userName, " IDENTIFIED BY '", userPassword, "';");
		PREPARE Stmt FROM @SQLStmt;
		EXECUTE Stmt;
		DEALLOCATE PREPARE Stmt;
		
		FLUSH PRIVILEGES;
	END IF;
END$$
DELIMITER ;

