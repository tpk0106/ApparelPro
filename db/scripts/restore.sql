-- A .bak file remembers the exact file paths it was originally backed up
-- from (e.g. C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\...),
-- which won't exist inside this Linux container. This script asks the
-- backup file itself what its logical files are called (RESTORE
-- FILELISTONLY), then builds the MOVE clause and RESTORE DATABASE statement
-- dynamically so it works regardless of the original file layout.
SET NOCOUNT ON;

DECLARE @BackupFile NVARCHAR(500) = N'$(BACKUPFILE)';
DECLARE @DbName     NVARCHAR(128) = N'$(DBNAME)';
DECLARE @DataPath   NVARCHAR(500) = N'/var/opt/mssql/data/';

IF DB_ID(@DbName) IS NOT NULL
BEGIN
    PRINT 'Database ' + @DbName + ' already exists — skipping restore.';
    RETURN;
END

IF OBJECT_ID('tempdb..#FileList') IS NOT NULL DROP TABLE #FileList;

CREATE TABLE #FileList (
    LogicalName         NVARCHAR(128), PhysicalName NVARCHAR(500), [Type] CHAR(1),
    FileGroupName        NVARCHAR(128) NULL, Size NUMERIC(20,0), MaxSize NUMERIC(20,0),
    FileId               BIGINT, CreateLSN NUMERIC(25,0), DropLSN NUMERIC(25,0) NULL,
    UniqueId             UNIQUEIDENTIFIER, ReadOnlyLSN NUMERIC(25,0) NULL, ReadWriteLSN NUMERIC(25,0) NULL,
    BackupSizeInBytes    BIGINT, SourceBlockSize INT, FileGroupId INT, LogGroupGUID UNIQUEIDENTIFIER NULL,
    DifferentialBaseLSN  NUMERIC(25,0) NULL, DifferentialBaseGUID UNIQUEIDENTIFIER NULL,
    IsReadOnly           BIT, IsPresent BIT, TDEThumbprint VARBINARY(32) NULL, SnapshotUrl NVARCHAR(360) NULL
);

INSERT INTO #FileList
EXEC('RESTORE FILELISTONLY FROM DISK = ''' + @BackupFile + '''');

DECLARE @MoveClause NVARCHAR(MAX) = '';

SELECT @MoveClause = @MoveClause +
    ', MOVE ''' + LogicalName + ''' TO ''' + @DataPath + @DbName +
    CASE
        WHEN [Type] = 'L' THEN '_log.ldf'
        WHEN ROW_NUMBER() OVER (PARTITION BY [Type] ORDER BY FileId) = 1 THEN '.mdf'
        ELSE '_' + CAST(FileId AS NVARCHAR) + '.ndf'
    END + ''''
FROM #FileList;

DECLARE @Sql NVARCHAR(MAX) =
    'RESTORE DATABASE [' + @DbName + '] FROM DISK = ''' + @BackupFile + '''' +
    ' WITH ' + STUFF(@MoveClause, 1, 2, '') + ', REPLACE, STATS = 10;';

PRINT @Sql;
EXEC(@Sql);
