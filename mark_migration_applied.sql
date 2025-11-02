-- ============================================
-- Mark EF Core Migration as Applied
-- ============================================
-- Run this AFTER creating the initial migration with:
-- dotnet ef migrations add InitialCreate
-- ============================================

USE MedicalTriageDB;
GO

-- Create the EF Migrations history table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'EF Migrations history table created.';
END
ELSE
BEGIN
    PRINT 'EF Migrations history table already exists.';
END
GO

-- Insert the initial migration record
-- Replace 'InitialCreate' with your actual migration name if different
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = 'InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('InitialCreate', '8.0.10');
    PRINT 'Migration InitialCreate marked as applied.';
END
ELSE
BEGIN
    PRINT 'Migration InitialCreate already exists in history.';
END
GO

PRINT '';
PRINT '============================================';
PRINT 'Migration tracking setup complete!';
PRINT 'You can now use your application.';
PRINT '============================================';

