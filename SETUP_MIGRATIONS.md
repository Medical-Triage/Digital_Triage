# Setting Up Entity Framework Migrations

Since you created the database tables manually, you need to tell EF Core that the database is already migrated.

## Step 1: Create Initial Migration (without applying it)

Run this command in the Package Manager Console or terminal:

```bash
dotnet ef migrations add InitialCreate
```

This creates a migration file that represents your current database schema.

## Step 2: Mark Migration as Applied (without running it)

Since your database already has the tables, you need to tell EF Core that the migration is already applied:

```bash
dotnet ef database update --connection "Data Source=VICTOR\SQLEXPRESS01;Initial Catalog=MedicalTriageDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True" --migration InitialCreate --no-build
```

**OR** manually insert the migration record into the database:

Run this SQL in SSMS:

```sql
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
END
GO

-- Insert the initial migration record
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = 'InitialCreate')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('InitialCreate', '8.0.10');
    PRINT 'Migration InitialCreate marked as applied.';
END
ELSE
BEGIN
    PRINT 'Migration InitialCreate already exists.';
END
GO
```

## Step 3: Test Your Application

After this, your application should work! You can:
1. Test the login functionality
2. Register a new user
3. Access the admin dashboard

## Future Changes

For any future schema changes (adding columns, tables, etc.):
1. Modify your C# models
2. Run: `dotnet ef migrations add MigrationName`
3. Run: `dotnet ef database update`
4. EF Core will apply only the new changes

