-- Creates the QrSanad database on SQL Server. Run once, then run QrSanad.SqlServer.sql against it.
-- Both scripts are safe to re-run.

IF DB_ID(N'QrSanadDb') IS NULL
BEGIN
    CREATE DATABASE [QrSanadDb];
END;
GO

-- Snapshot isolation lets readers proceed without blocking on writers.
ALTER DATABASE [QrSanadDb] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO

USE [QrSanadDb];
GO

-- Optional: a least-privilege login for the API instead of sa/dbo. Replace the password, or
-- use a Windows/Entra identity, before running.
--
-- CREATE LOGIN [qrsanad_api] WITH PASSWORD = N'<strong-password>';
-- CREATE USER  [qrsanad_api] FOR LOGIN [qrsanad_api];
-- ALTER ROLE db_datareader ADD MEMBER [qrsanad_api];
-- ALTER ROLE db_datawriter ADD MEMBER [qrsanad_api];
--
-- Applying migrations from the API (ApplyMigrationsOnStartup) additionally needs db_ddladmin;
-- prefer running QrSanad.SqlServer.sql as a deployment step and keeping the API on read/write.
