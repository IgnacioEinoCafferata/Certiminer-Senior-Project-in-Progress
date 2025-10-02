SELECT DB_NAME() AS CurrentDB;               -- debe decir Certiminer
SELECT * FROM sys.tables WHERE name='Folders';
EXEC sp_help 'dbo.Videos';                   -- debe listar la columna FolderId
EXEC sp_help 'dbo.Tests';                    -- idem


-- Create Folders table
IF OBJECT_ID(N'dbo.Folders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Folders
    (
        Id       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name     NVARCHAR(128) NOT NULL,
        Kind     INT NOT NULL, -- 1=Tests, 2=Videos
        ParentId INT NULL
    );
    ALTER TABLE dbo.Folders
      ADD CONSTRAINT FK_Folders_Parent
      FOREIGN KEY (ParentId) REFERENCES dbo.Folders(Id) ON DELETE NO ACTION;
END
GO

-- Add FolderId to Tests
IF COL_LENGTH('dbo.Tests', 'FolderId') IS NULL
BEGIN
    ALTER TABLE dbo.Tests ADD FolderId INT NULL;
    ALTER TABLE dbo.Tests ADD CONSTRAINT FK_Tests_Folder
        FOREIGN KEY (FolderId) REFERENCES dbo.Folders(Id) ON DELETE SET NULL;
END
GO

-- Add FolderId to Videos
IF COL_LENGTH('dbo.Videos', 'FolderId') IS NULL
BEGIN
    ALTER TABLE dbo.Videos ADD FolderId INT NULL;
    ALTER TABLE dbo.Videos ADD CONSTRAINT FK_Videos_Folder
        FOREIGN KEY (FolderId) REFERENCES dbo.Folders(Id) ON DELETE SET NULL;
END
GO
