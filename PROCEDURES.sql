CREATE OR ALTER PROCEDURE RepairTime
		@Transport AS VARCHAR(255), @Begin AS DATE, @End AS DATE
AS
DECLARE @tmp AS Int = 0
SELECT @tmp = COUNT(DATEDIFF(DAY, Request_Date, End_Date)) FROM dbo.Repair 
												WHERE Request_Date >= @Begin AND Request_Date <= @End AND Transport = @Transport
RETURN @tmp
GO
DECLARE @d1 AS DATE = DATEADD(YEAR, -10, getdate()), @d2 AS DATE  = GETDATE(), @n AS INT = 0

EXEC @n = RepairTime @Transport = '82KNFDL', @Begin = @d1, @End = @d2
SELECT @n, @d1, @d2;
SELECT COUNT(DATEDIFF(DAY, Request_Date, End_Date)) FROM dbo.Repair 
												WHERE Request_Date >= @d1 AND Request_Date <= @d2
------------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE WorkerStatistics 
							@cur CURSOR VARYING OUTPUT, @Begin AS DATE
AS
SET NOCOUNT ON;
SET @cur = CURSOR
    FORWARD_ONLY STATIC FOR 
	SELECT Drivers.Name, Number_of_days, Companies.Name FROM dbo.Drivers INNER JOIN (SELECT driver, COUNT(*) AS Number_of_days 
											FROM dbo.Work_Scheldue WHERE Start_Time > @Begin GROUP BY Driver) AS W ON Drivers.Id = W.Driver 
												INNER JOIN Companies ON Companies.Id = Drivers.Company 

------------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE RepairStatistics 
						@Begin AS DATE, @End AS DATE--, @Cursor CURSOR VARYING OUTPUT
AS
--SET NOCOUNT ON;
SELECT top 10 Transport.Number, 
	   avg(DATEDIFF(DAY, Request_Date, End_Date)) --FROM Repair as R1 where R1.Transport = Number) 
	   AS Count_Of_Days,  
	   (select avg(Rep) from (SELECT transport, (SELECT TOP 1 DATEDIFF(DAY, repair.Request_Date, R2.End_Date) as diff FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = Number ORDER BY id) As Rep
									FROM dbo.Repair where Transport = Number AND Request_Date >= @Begin And Request_Date <= @End ) as S )
									AS Avg_time_without_repair 
													FROM (dbo.Repair RIGHT JOIN dbo.Transport ON Transport.Number = Repair.Transport)
															where Request_Date >= @Begin And Request_Date <= @End group by Number
													
RETURN
GO
DECLARE @d1 AS DATE = DATEADD(YEAR, -40, getdate()), @d2 AS DATE  = GETDATE(), @n AS INT = 0
EXEC RepairStatistics @Begin = @d1, @End = @d2

-----------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE CreateCur 
							@cur CURSOR VARYING OUTPUT
AS
SET NOCOUNT ON;
SET @cur = CURSOR 
FORWARD_ONLY STATIC FOR 
select top 100 Transport, avg(Rep) as AVG from (SELECT transport, (SELECT TOP 1 DATEDIFF(DAY, repair.Request_Date, R2.End_Date) as diff FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = dbo.Repair.Transport ORDER BY id) As Rep
										FROM dbo.Repair) as S group by Transport order by AVG
OPEN @cur;
go
declare @s as varchar(255), @MyCursor CURSOR, @avg as INT;
EXEC CreateCur @MyCursor OUTPUT;
WHILE (@@FETCH_STATUS = 0)
BEGIN;
     FETCH NEXT FROM @MyCursor into @s, @avg;
	 print @s + ' ' + str(@avg)
END;
FETCH NEXT FROM @MyCursor;
CLOSE @MyCursor;
DEALLOCATE @MyCursor;
GO
------------------------------------------------------------------------------------------------------
--SELECT DATEDIFF(DAY, Request_Date, End_Date) FROM dbo.Repair where transport = '00AAWKJ' 

--DECLARE @d1 AS DATE = DATEADD(YEAR, -40, getdate()), @d2 AS DATE  = GETDATE(), @n AS INT = 0
--SELECT TOP 100 transport, DATEDIFF(DAY, repair.Request_Date, (SELECT TOP 1 R2.End_Date FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = dbo.Repair.Transport ORDER BY id )) AS T 
--										FROM dbo.Repair ORDER BY transport
--select top 100 Transport, avg(Rep) from (SELECT transport, (SELECT TOP 1 DATEDIFF(DAY, repair.Request_Date, R2.End_Date) as diff FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = dbo.Repair.Transport AND Request_Date >= @d1 And End_Date <= @d2  ORDER BY id) As Rep
--										FROM dbo.Repair Where Request_Date >= @d1 And End_Date <= @d2 ) as S group by Transport

--SELECT TOP 100 transport, (SELECT TOP 1 DATEDIFF(DAY, repair.Request_Date, R2.End_Date) FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = dbo.Repair.Transport ORDER BY id) AS T 
--										FROM dbo.Repair where transport = '00AAWKJ'ORDER BY id

--SELECT TOP 100 transport, (SELECT TOP 1 DATEDIFF(DAY, repair.Request_Date, R2.End_Date) FROM Repair AS R2 WHERE R2.id > Repair.id AND R2.transport = dbo.Repair.Transport ORDER BY id) AS T 
--										FROM dbo.Repair ORDER BY Transport
CREATE OR ALTER PROCEDURE CreateCur_W_S 
			@Number AS varchar(255), @Name as varchar(255), @Route as Int
AS
select Work_Scheldue.Id, Start_Time, Additional_Staff.Name from Work_Scheldue inner join Additional_Staff
											 on Work_Scheldue.Additional_Staff = Additional_Staff.Id
												inner join Drivers on Drivers.Id = Work_Scheldue.Driver
													where Drivers.Name = @Name AND @Number = Transport
														AND @Route = Route_No order by id
Go											
Declare @number AS varchar(255) = '82KNFDL', @name as varchar(255) = 'Margarita Murray', @route as Int = 243107812;
exec CreateCur_W_S @number, @name, @route

select * from Work_Scheldue where Transport = '82KNFDL'
select * from Drivers where Id = 958
-----------------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE Send_Driver_On_Vacation 
			@Name as varchar(255) = null, @Id As int = null
AS
declare @tmp as int;
if @Name is null 
	if @id is null
	begin
		RAISERROR('Транспорт, Водитель или Маршрут не принадлежат одной компании', 16, 10)
	end
	else 
		select @tmp = id from Drivers where @id = id
else 
	select Top 1 @tmp = id from Drivers where @Name = Name

update Drivers set state = 'On Vacation' where @tmp = id
Delete from Work_Scheldue where Driver = @tmp AND Start_Time > GETDATE() 
PRINT 'Done' 
Go
update Drivers set state = null where Id = 14237
INSERT INTO dbo.Work_Scheldue ( Route_No, Transport, Additional_Staff, Driver, Start_Time ) 
	VALUES ( 1205, '34UUIVT', NULL, 14237, DATEADD(DAY, 7, GETDATE()) ); 
	select top 10 * from Work_Scheldue order by Id desc
exec Send_Driver_On_Vacation 
exec Send_Driver_On_Vacation @Id = 14237