create database ServiceEPSI_V8;
go

use ServiceEPSI_V8;
go

-- Création des tables

create table DatabaseServerName
(
	Id int not null
		constraint PK_DatabaseServerName
			primary key,
	Code varchar(15),
	Name varchar(30) not null,
	IPLocale varchar(20) not null,
	NomDNS varchar(50) not null,
	Description varchar(200),
	CanAddDatabase int default 1 not null,
	PortLocal int,
	PortExterne int,
	NomDNSLocal varchar(50)
)
go

create table DatabaseDB
(
	Id int identity
		constraint PK_DatabaseDB
			primary key,
	ServerId int not null
		constraint FK_DatabaseDB_DatabaseServerName
			references DatabaseServerName,
	NomBD varchar(50) not null,
	DateCreation datetime,
	Commentaire varchar(max)
)
go

create table DatabaseGroupUser
(
	DbId int not null
		constraint FK_DatabaseGroupUser_DatabaseDB
			references DatabaseDB,
	SqlLogin varchar(30) not null,
	UserLogin varchar(30),
	UserFullName varchar(60),
	GroupType int not null,
	AddedByUserLogin varchar(30) not null,
	constraint PK_DatabaseGroupUser
		primary key (DbId, SqlLogin)
)
go

create table DatabaseServerUser
(
	ServerId int not null
		constraint FK_DatabaseServerUser_DatabaseServerName
			references DatabaseServerName,
	SqlLogin varchar(30) not null,
	UserLogin varchar(30) not null,
	constraint PK_DatabaseUserServer
		primary key (ServerId, SqlLogin)
)
go


-- Insertion des données

insert into dbo.DatabaseServerName (Id, Code, Name, IPLocale, NomDNS, Description, CanAddDatabase, PortLocal, PortExterne, NomDNSLocal) values (4, N'SQLSERVER', N'SQL Server 2', N'192.168.100.17', N'sqlserver2.montpellier.epsi.fr', N'', 1, 1433, 4453, N'sqlserver2.montpellier.lan');
insert into dbo.DatabaseServerName (Id, Code, Name, IPLocale, NomDNS, Description, CanAddDatabase, PortLocal, PortExterne, NomDNSLocal) values (5, N'ORACLE', N'Oracle 2', N'192.168.100.17', N'oracle2.montpellier.epsi.fr', null, 0, 1521, 4531, N'
oracle2.montpellier.lan');
insert into dbo.DatabaseServerName (Id, Code, Name, IPLocale, NomDNS, Description, CanAddDatabase, PortLocal, PortExterne, NomDNSLocal) values (6, N'MYSQL', N'MySQL 2', N'192.168.100.7', N'mysql2.montpellier.epsi.fr', null, 1, 3306, 5306, N'mysql');

insert into dbo.DatabaseServerUser (ServerId, SqlLogin, UserLogin) values (4, N'test.v5', N'test.v5');
insert into dbo.DatabaseServerUser (ServerId, SqlLogin, UserLogin) values (4, N'test.v8', N'test.v8');
insert into dbo.DatabaseServerUser (ServerId, SqlLogin, UserLogin) values (6, N'test.v5', N'test.v5');
insert into dbo.DatabaseServerUser (ServerId, SqlLogin, UserLogin) values (6, N'test.v8', N'test.v8');


SET IDENTITY_INSERT dbo.DatabaseDB ON;
insert into dbo.DatabaseDB (Id, ServerId, NomBD, DateCreation, Commentaire) values (1, 6, N'test20191007', N'2019-10-07 14:37:51.303', null);
insert into dbo.DatabaseDB (Id, ServerId, NomBD, DateCreation, Commentaire) values (2, 6, N'Test N1', N'2020-05-14 14:06:29.327', null);
insert into dbo.DatabaseDB (Id, ServerId, NomBD, DateCreation, Commentaire) values (3, 6, N'Test Autre', N'2020-05-14 19:11:39.777', null);
SET IDENTITY_INSERT dbo.DatabaseDB OFF;

insert into dbo.DatabaseGroupUser (DbId, SqlLogin, UserLogin, UserFullName, GroupType, AddedByUserLogin) values (1, N'test.v8', N'test.v8', N'V8 Test', 1, N'test.v8');
insert into dbo.DatabaseGroupUser (DbId, SqlLogin, UserLogin, UserFullName, GroupType, AddedByUserLogin) values (2, N'test.v5', N'test.v5', N'v5 Test', 1, N'test.v5');
insert into dbo.DatabaseGroupUser (DbId, SqlLogin, UserLogin, UserFullName, GroupType, AddedByUserLogin) values (2, N'test.v5ee', N'test.v5ee', null, 4, N'test.v5');
insert into dbo.DatabaseGroupUser (DbId, SqlLogin, UserLogin, UserFullName, GroupType, AddedByUserLogin) values (2, N'test.v8', N'test.v8', N'V8 Test', 4, N'test.v5');
insert into dbo.DatabaseGroupUser (DbId, SqlLogin, UserLogin, UserFullName, GroupType, AddedByUserLogin) values (3, N'test.v5', N'test.v5', N'v5 Test', 1, N'test.v5');


