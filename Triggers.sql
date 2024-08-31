--DROP TABLE dbo.History 
--CREATE TABLE History (
--	id INT IDENTITY(1,1), 
--	TYPE varchar(6) NOT NULL,
--	Info VARCHAR(255) NOT NULL, 
--	Event_Time datetime NOT NULL
--)
--CREATE TABLE CompanyStates( id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, TYPE varchar(255) NOT NULL)
--ALTER TABLE dbo.Companies ADD state varchar(255)
--ALTER TABLE dbo.Drivers ADD state varchar(255)
--ALTER TABLE dbo.Transport ADD state varchar(255)

DROP TRIGGER IF EXISTS Company_Check 
go

CREATE TRIGGER Company_Check ON Work_Scheldue FOR INSERT, UPDATE 
AS
BEGIN
	IF NOT EXISTS (SELECT Company FROM dbo.Routes INNER join Inserted ON Inserted.Route_No = Routes.Route_No 
					INTERSECT SELECT Company FROM dbo.Transport INNER join Inserted ON Transport.Number = Inserted.Transport
					INTERSECT SELECT Company FROM dbo.Drivers INNER join Inserted on dbo.Drivers.Id = Inserted.Driver)
					BEGIN 
						RAISERROR('Транспорт, Водитель или Маршрут не принадлежат одной компании', 16, 10)
						ROLLBACK TRANSACTION
					END
	ELSE IF EXISTS (SELECT * FROM dbo.Transport INNER JOIN Inserted ON Transport.Number = Inserted.Transport WHERE Transport.state IS NOT NULL)
			BEGIN
				RAISERROR('Транспорт не доступен', 16, 10)        
			END 
	ELSE IF EXISTS(SELECT * FROM dbo.Drivers INNER JOIN Inserted ON Drivers.id = Inserted.Driver WHERE Drivers.state IS NOT NULL)
			BEGIN
				RAISERROR('Водитель не доступен', 16, 10)        
			END 
END 
SELECT TOP 10 * FROM dbo.Work_Scheldue ORDER BY id desc
INSERT INTO dbo.Work_Scheldue ( Route_No, Transport, Additional_Staff, Driver, Start_Time ) 
	VALUES ( 1205, '34UUIVT', NULL, 1428, DATEADD(DAY, -7, GETDATE()) )
	SELECT Company FROM dbo.Routes Right JOIN (SELECT * FROM dbo.Work_Scheldue WHERE Transport = '34UUIVT') AS w ON w.Route_No = Routes.Route_No 
 INTERSECT SELECT Company FROM dbo.Transport Right JOIN (SELECT * FROM dbo.Work_Scheldue WHERE Transport = '34UUIVT') AS w ON Transport.Number = w.Transport
 INTERSECT SELECT Company FROM dbo.Drivers JOIN (SELECT * FROM dbo.Work_Scheldue WHERE Transport = '34UUIVT') AS w on dbo.Drivers.Id = w.Driver
--------------------------------------------------------------------------
DROP TRIGGER IF EXISTS Save_Event 
go
CREATE TRIGGER Save_Event ON dbo.Companies
INSTEAD OF UPDATE, DELETE
AS 
	DECLARE @name VARCHAR(255), @id INT, @state varchar(255)
	DECLARE cur CURSOR FOR SELECT * FROM Deleted

	IF Exists (SELECT * FROM Inserted)
		BEGIN
			OPEN cur
			FETCH NEXT FROM cur INTO @id, @name, @state
			WHILE @@FETCH_STATUS = 0 
			BEGIN 
				INSERT INTO History (TYPE, info, Event_Time) VALUES 
								('UPDATE', CAST (@id AS VARCHAR(255)) + '; ' + @name + '; ' + ISNULL(@state, 'Null'), GETDATE())
				UPDATE dbo.Companies SET Name = @name, state = @state WHERE id = @id
				FETCH NEXT FROM cur INTO @id, @name, @state
			END 
		END 
	ELSE 
		OPEN cur
		FETCH NEXT FROM cur INTO @id, @name, @state
		WHILE @@FETCH_STATUS = 0 
		BEGIN
		  		--PRINT CAST (@id AS varchar) + '; ' + @name + '; ' + @state
			INSERT INTO History (TYPE, info, Event_Time) VALUES 
							('DELETE', CAST (@id AS VARCHAR(255)) + '; ' + @name + '; ' + ISNULL(@state, 'Null'), GETDATE())
				
			IF EXISTS (SELECT * FROM dbo.Transport INNER JOIN dbo.Drivers ON Drivers.Company = Transport.Company
									WHERE Drivers.Company = @id)
				BEGIN 
					UPDATE Drivers SET Drivers.State = 'Компания прекратила работу' WHERE Company = @id
					UPDATE Transport SET Transport.State = 'Компания прекратила работу' WHERE Company = @id
					UPDATE dbo.Companies SET State = 'Компания прекратила работу' WHERE id = @id
				END
			FETCH NEXT FROM cur INTO @id, @name, @state
		END 
	CLOSE cur
	DEALLOCATE cur

--DELETE FROM History
UPDATE dbo.Companies SET Name = Name + ' Hello' WHERE id = 11
SELECT * FROM dbo.History
SELECT * FROM dbo.Companies  WHERE id = 11

DELETE FROM dbo.Companies WHERE id = 10
SELECT * FROM dbo.Companies INNER JOIN dbo.Drivers ON Drivers.Company = Companies.Id 
							INNER JOIN dbo.Transport ON Transport.Company = Companies.Id WHERE Companies.Id = 10
SELECT * FROM dbo.History
-----------------------------------------------------------------------------------------------------
DROP TRIGGER IF EXISTS CHECK_Scheldue
go
CREATE TRIGGER CHECK_Scheldue ON dbo.Repair
INSTEAD OF INSERT 
AS
--IF NOT EXISTS (SELECT * FROM Transport INNER JOIN Inserted ON Inserted.Transport = Transport.Number
--								WHERE state IS NULL)
	BEGIN 
		DECLARE @num VARCHAR(255), @num2 VARCHAR(255),
				@date DATETIME, @date2 DATETIME,
				@id INT, @id2 INT,
				@comp INT, 
				@n INT,
				@replace_Transport VARCHAR(255)
		DECLARE cur1 CURSOR FOR SELECT id, Number, Company, Inserted.Request_Date FROM (dbo.Transport INNER JOIN Inserted ON Inserted.Transport = Transport.Number)
																			ON Transport.Number = Inserted.Transport)
		OPEN cur1
		FETCH NEXT FROM cur1 INTO @id, @num, @comp, @date
		WHILE @@FETCH_STATUS = 0
			BEGIN
				UPDATE dbo.Transport SET state = 'На ремонте: ' + CAST(@id AS VARCHAR(255)) WHERE Number = @num 

				IF EXISTS (SELECT TOP 1 Number FROM (SELECT * FROM dbo.Work_Scheldue where CONVERT(DATE, Start_Time) > CONVERT(DATE, @date)) AS W 
																	RIGHT JOIN dbo.Transport ON Transport.Number = W.Transport 
																		WHERE Company = @comp AND id IS NULL AND state IS null)
					UPDATE dbo.Work_Scheldue SET Transport = @replace_Transport WHERE Start_Time > @date
				ELSE 
				IF EXISTS (SELECT Number FROM (SELECT * FROM dbo.Work_Scheldue where CONVERT(DATE, Start_Time) > CONVERT(DATE, @date)) AS W 
																	RIGHT JOIN dbo.Transport ON Transport.Number = W.Transport 
																		WHERE  Number != @num AND Company = @comp AND id IS NOT NULL AND state IS null)
					BEGIN 
						DECLARE cur2 CURSOR FOR SELECT Number, W.Start_Time, W.id FROM (SELECT * FROM dbo.Work_Scheldue where CONVERT(DATE, Start_Time) > CONVERT(DATE, @date)) AS W 
																	RIGHT JOIN dbo.Transport ON Transport.Number = W.Transport 
																		WHERE Number != @num AND Company = @comp AND id IS NOT NULL AND state IS NULL ORDER BY W.Start_Time desc
						PRINT 'hello'
                        OPEN cur2
						FETCH NEXT FROM cur2 INTO @num2, @date2, @id2
						SELECT @n = COUNT(*) FROM dbo.Work_Scheldue WHERE Convert(DATE, Start_Time) > @date AND Transport = @num
						WHILE @@FETCH_STATUS = 0 AND @n > 0
						BEGIN
							PRINT 'hello ' + @num2 + ' ' + CAST (@n AS VARCHAR(255)) + ' ' + CAST (@id2 AS VARCHAR(255)) + ' ' + @num2
							--update dbo.Work_Scheldue SET Transport = @num2 WHERE Transport = @num AND
							--													 CONVERT(DATE, Start_Time) > CONVERT(DATE, @date) AND 
							--													 CONVERT(DATE, Start_Time) < CONVERT(DATE, @date2) 
							UPDATE dbo.Work_Scheldue SET Transport = @num2 WHERE Transport = @num AND 
													CONVERT(DATE, Start_Time) != ANY(SELECT CONVERT(DATE, W.Start_Time) FROM dbo.Work_Scheldue AS W 
																			WHERE W.Transport = @num2 AND CONVERT(DATE, W.Start_Time) > CONVERT(DATE, @date))
							SET @n = @n - 1
							FETCH NEXT FROM cur2 INTO @num2, @date2, @id2
						END 
						IF @n > 0 
							DELETE FROM dbo.Work_Scheldue WHERE CONVERT(DATE, Start_Time) >= CONVERT(DATE, @date2) AND Transport = @num
						CLOSE cur2
						DEALLOCATE cur2
					END 
				ELSE
					BEGIN 
						DELETE FROM dbo.Work_Scheldue WHERE Transport = @num
						PRINT 'hello2'
					end
				FETCH NEXT FROM cur1 INTO @id, @num, @comp, @date
			END
			PRINT 'hello3'

			CLOSE cur1
			DEALLOCATE cur1
	END

	INSERT INTO dbo.Repair ( Type, Transport, Request_Date, Status, Engineer, End_Date)
				VALUES	( '', '34UUIVT', DATEADD(DAY, -8, GETDATE()), 0, 0, NULL ) 
		SELECT TOP 10 * FROM dbo.Repair ORDER BY id DESC

		SELECT TOP 10 * FROM dbo.Work_Scheldue ORDER BY id DESC
		
		

-------------------------------------------- TEST --------------------------------------------



DELETE FROM dbo.Work_Scheldue WHERE id = ANY(SELECT Work_Scheldue.id FROM dbo.Work_Scheldue INNER JOIN dbo.Transport ON Transport.Number = Work_Scheldue.Transport WHERE Company = 1001)
----UPDATE dbo.Transport SET state = NULL WHERE Number = '34UUIVT' 
INSERT INTO dbo.Work_Scheldue ( Route_No, Transport, Additional_Staff, Driver, Start_Time ) 
	VALUES ( 1205, '34UUIVT', NULL, 14237, DATEADD(DAY, -7, GETDATE()) ),  
		   ( 1205, '36NUIVT', NULL, 14237, DATEADD(DAY, -7, GETDATE()) ), 
		   ( 1205, '39EFIVT', NULL, 14237, DATEADD(DAY, -6, GETDATE()) ),
		   ( 1205, '34UUIVT', NULL, 14237, DATEADD(DAY, -6, GETDATE()) )
		   SELECT * FROM dbo.Transport WHERE Number = '39EFIVT'
SELECT * FROM (SELECT * FROM dbo.Work_Scheldue where Convert(DATEtime, Start_Time) >= DATEADD(DAY, -8, GETDATE())) AS W 
																	RIGHT JOIN dbo.Transport ON Transport.Number = W.Transport 
																		WHERE Company = 1001 AND id IS NOT NULL AND state IS null
SELECT COUNT(*) FROM dbo.Work_Scheldue WHERE Convert(DATE, Start_Time) > DATEADD(DAY, -8, GETDATE()) AND Transport = '34UUIVT'

--INSERT INTO dbo.Companies ( Name, state ) VALUES ('ИП Даниил Кабанов', NULL )
SELECT * FROM dbo.Companies
--INSERT INTO dbo.Routes ( Route_No, Company ) VALUES ( 1205, 1001 )
--INSERT INTO Drivers VALUES ('Кабанов Даниил', 1001, NUll)
SELECT TOP 1 * FROM dbo.Drivers ORDER BY id desc
------------------------------------------------
DROP TRIGGER IF EXISTS 
CREATE TRIGGER DEl_Route ON Routes 
AFTER DELETE
AS
BEGIN
	RAISERROR('Невозможно удалить маршрут', 10, 12)
	ROLLBACK TRANSACTION
END 