CREATE TABLE [Transport] 
(
	Number varchar(255) NOT NULL
	CONSTRAINT PK_Transport PRIMARY KEY,
	Type varchar(255) NOT NULL,
	Company integer NOT NULL,
)
GO
CREATE TABLE [Companies] (
	Id integer NOT NULL
	CONSTRAINT PK_Companies PRIMARY KEY,
	Name varchar(255) NOT NULL UNIQUE,
)
GO
CREATE TABLE [Drivers] (
	Id integer NOT NULL
	CONSTRAINT PK_Drivers PRIMARY KEY,
	Name varchar(255) NOT NULL,
	Company integer NOT NULL,
)
GO
CREATE TABLE [Additional_Staff] 
(
	Id integer NOT NULL
	CONSTRAINT PK_Additional_Staff PRIMARY KEY,
	Name varchar(255) NOT NULL,
)
GO
CREATE TABLE [Stopps] (
	Id integer NOT NULL
	CONSTRAINT PK_Stopps PRIMARY KEY,
	Street varchar(255) NOT NULL,
	Stop_Addres varchar(255) NOT NULL,
	Stop_Time time NOT NULL,
)
GO
CREATE TABLE [Routes] (
	Route_No integer NOT NULL
	CONSTRAINT PK_Routes PRIMARY KEY,
	Company integer NOT NULL,
)
GO
CREATE TABLE [Route_Stop] (
	Stop integer NOT NULL,
	Route_No integer NOT NULL,
	Travel_time time NOT NULL,
	Stop_Serial_Number integer NOT NULL,
	PRIMARY KEY (Stop, Route_No)
)
GO
CREATE TABLE [Work_Scheldue] (
	Id integer NOT NULL 
	CONSTRAINT PK_WORK_SCHELDUE PRIMARY KEY,
	Route_No integer NOT NULL,
	Transport varchar(255) NOT NULL,
	Start_Date date NOT NULL,
	Start_Time timestamp NOT NULL,
	Additional_Staff integer,
	Driver integer NOT NULL
)
GO

CREATE TABLE [Repair] (
	Id varchar(255) NOT NULL
	CONSTRAINT PK_REPAIR PRIMARY KEY,
	Type varchar(255) NOT NULL,
	Transport varchar(255) NOT NULL,
	Request_Date date NOT NULL,
	Status integer NOT NULL,
	Engineer integer NOT NULL,
	End_Date date NOT NULL
)
GO
CREATE TABLE [Payment] (
	Type integer NOT NULL 
	CONSTRAINT PK_PAYMENT PRIMARY KEY,
	Cost float NOT NULL
)
GO
CREATE TABLE [Cost_Route] (
	Id integer NOT NULL 
	CONSTRAINT PK_COST_ROUTE PRIMARY KEY,
	Route_No integer NOT NULL,
	Payment integer NOT NULL,
	Start_Date date NOT NULL,
	End_Date date
)
GO

Alter table Transport ADD FOREIGN KEY(Company) references Companies(Id)
GO
Alter table Drivers ADD FOREIGN KEY(Company) references Companies(Id)
GO
Alter table Routes ADD FOREIGN KEY(Company) references Companies(Id)
GO
Alter table Route_Stop ADD FOREIGN KEY(Route_No) references Routes(Route_No)
GO
Alter table Route_Stop ADD FOREIGN KEY(Stop) references Stopps(Id)
GO
Alter table Work_Scheldue ADD FOREIGN KEY(Route_No) references Routes(Route_No)
GO
Alter table Work_Scheldue ADD FOREIGN KEY(Route_No) references Routes(Route_No)
GO
Alter table Work_Scheldue ADD FOREIGN KEY(Transport) references Transport(Number)
GO
Alter table Work_Scheldue ADD FOREIGN KEY(Driver) references Drivers(Id)
GO
Alter table Work_Scheldue ADD FOREIGN KEY(Additional_Staff) references Additional_Staff(Id)
GO
Alter table Cost_Route ADD FOREIGN KEY(Route_No) references Routes(Route_No)
GO
Alter table Cost_Route ADD FOREIGN KEY(Payment) references Payment(Type)
GO
Alter table Repair ADD FOREIGN KEY(Transport) references Transport(Number)
GO
Alter table Route_Stop ADD Previous_Route int not null, Previous_Stop int not null
GO
Alter table Route_Stop ADD FOREIGN KEY(Previous_Route, Previous_Stop) references Route_Stop(Route_No, Stop)
GO
---------------------------------------------------------------------------------
-- insert into Companies (Id, Name) values (0, 'Транспорт Саратов');
-- Go
-- INSERT Into Companies (Id, Name) Values (1, 'ИП Джашитов'), (2, 'ОАО Майкрософт');
-- GO
-- INSERT Into Drivers (Id, Name, Company) Values (0, 'Кабанов Даниил Дмитриевич', 1), (1, 'Крылов Роман Алексеевич',  1);
-- Go
-- insert into Routes (Route_No, Company) values (1, 2), (5, 0)
-- go
-- INSERT Into Companies (Id, Name) Values (3, 'ООО Яндекс');
-- GO
-- INSERT Into Transport (Number, Type, Company) values ('М917УВ', 'Автобус', 2), ('А375УР', 'Автобус', 2), ('А711ВТ', 'Автобус', 3), 
-- ('К598УР', 'Троллейбус', 1), ('О848ЕЕ', 'Троллейбус', 1);
-- GO 
-- Delete Companies where id > 1;
-- Go
-- Delete Drivers where Company = 1;
-- Go
