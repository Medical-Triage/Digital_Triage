# Digital Triage App

A modern medical triage platform built with ASP.NET Core Blazor Server that enables patients to manage their medical information, receive AI-powered triage recommendations, and allows healthcare professionals to monitor patient data through an admin dashboard.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Technology Stack](#technology-stack)
- [Installation](#installation)
- [Database Configuration](#database-configuration)
- [Running the Application](#running-the-application)
- [How the Application Works](#how-the-application-works)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [User Roles](#user-roles)
- [Troubleshooting](#troubleshooting)

## Features

### For Patients
- **User Registration & Authentication**: Secure account creation with email and password (password hashing using BCrypt)
- **Personal Information Management**: Store and update personal details including CNP, citizenship, place of birth, and domicile address
- **Medical Information Management**: Record and manage medical data including:
  - Blood type
  - Allergies
  - Chronic diseases
  - Current medications
  - Emergency contact information
  - Last visit date
  - Triage category
- **AI Triage Assistant**: Get preliminary medical triage recommendations based on symptoms (Preview mode - UI only)
- **Patient Issues Tracking**: Submit and track medical issues/concerns

### For Doctors/Administrators
- **Admin Dashboard**: Access to comprehensive patient data and medical records
- **Patient Management**: View all registered patients and their information
- **Medical Records Access**: Full access to patient medical histories

## Prerequisites

Before running this application, ensure you have the following installed:

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **SQL Server** (SQL Server Express, Developer, or full edition) ([Download SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))
- **SQL Server Management Studio (SSMS)** (optional, for database management) ([Download SSMS](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms))
- **Visual Studio 2022** or **Visual Studio Code** (recommended for development)

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (Blazor Server)
- **Database**: SQL Server with Entity Framework Core 8.0.10
- **Authentication**: Cookie-based authentication with role-based authorization
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **UI Components**: Bootstrap 5 (via CDN), Bootstrap Icons
- **Rendering Mode**: Interactive Server Components

## Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DigitalTriageApp
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Configure the database connection** (see [Database Configuration](#database-configuration) section below)

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```
   
   If you need to create a new migration:
   ```bash
   dotnet ef migrations add <MigrationName>
   ```

## Database Configuration

The application uses SQL Server as its database. Follow these steps to configure the database:

### Step 1: Create the Database

1. Open **SQL Server Management Studio (SSMS)** or use `sqlcmd`
2. Connect to your SQL Server instance
3. Create a new database named `MedicalTriageDB`:
   ```sql
   CREATE DATABASE MedicalTriageDB;
   ```

### Step 2: Configure Connection String

1. Open `appsettings.Development.json` in the project root
2. Update the connection string to match your SQL Server configuration:

   ```json
   {
     "ConnectionStrings": {
       "MedicalTriageDb": "Data Source=<YOUR_SERVER_NAME>\\<INSTANCE_NAME>;Initial Catalog=MedicalTriageDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0"
     }
   }
   ```

   **Connection String Parameters:**
   - `<YOUR_SERVER_NAME>`: Your SQL Server name (e.g., `VICTOR`)
   - `<INSTANCE_NAME>`: Your SQL Server instance name (e.g., `SQLEXPRESS01`)
   - If using Windows Authentication, keep `Integrated Security=True`
   - For SQL Server Authentication, use:
     ```
     Data Source=SERVER\\INSTANCE;Initial Catalog=MedicalTriageDB;User Id=username;Password=password;Encrypt=True;TrustServerCertificate=True;
     ```

### Step 3: Create Database Schema

You have two options to create the database schema:

#### Option A: Using Entity Framework Migrations (Recommended)

After configuring the connection string, apply the database migrations:

```bash
dotnet ef database update
```

This will create all necessary tables automatically.

#### Option B: Using SQL Scripts (Manual Setup)

If you prefer to create the database schema manually using SQL scripts, you can use the following SQL commands. This is useful if you want full control over the database creation or if you don't have Entity Framework tools installed.

**Complete SQL Script:**

```sql
-- Use the database
USE MedicalTriageDB;
GO

-- Create Domiciles table
CREATE TABLE [dbo].[Domiciles] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Country] NVARCHAR(100) NULL,
    [County] NVARCHAR(100) NULL,
    [City] NVARCHAR(100) NULL,
    [Street] NVARCHAR(150) NULL,
    [Number] NVARCHAR(20) NULL,
    CONSTRAINT [PK_Domiciles] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Create PlacesOfBirth table
CREATE TABLE [dbo].[PlacesOfBirth] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Country] NVARCHAR(100) NULL,
    [County] NVARCHAR(100) NULL,
    [City] NVARCHAR(100) NULL,
    CONSTRAINT [PK_PlacesOfBirth] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Create Patients table
CREATE TABLE [dbo].[Patients] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Email] NVARCHAR(200) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [FirstName] NVARCHAR(100) NULL,
    [LastName] NVARCHAR(100) NULL,
    [Cnp] NVARCHAR(13) NULL,
    [Serie] NVARCHAR(2) NULL,
    [Nr] NVARCHAR(6) NULL,
    [Citizenship] NVARCHAR(100) NULL,
    [PlaceOfBirthId] INT NULL,
    [DomicileId] INT NULL,
    [Role] NVARCHAR(20) NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Patients_Domiciles_DomicileId] FOREIGN KEY ([DomicileId]) 
        REFERENCES [dbo].[Domiciles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Patients_PlacesOfBirth_PlaceOfBirthId] FOREIGN KEY ([PlaceOfBirthId]) 
        REFERENCES [dbo].[PlacesOfBirth] ([Id]) ON DELETE NO ACTION
);
GO

-- Create MedicalDatas table
CREATE TABLE [dbo].[MedicalDatas] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BloodType] NVARCHAR(5) NULL,
    [Allergies] NVARCHAR(1000) NULL,
    [ChronicDiseases] NVARCHAR(1000) NULL,
    [CurrentMedication] NVARCHAR(1000) NULL,
    [EmergencyContactName] NVARCHAR(200) NULL,
    [EmergencyContactPhone] NVARCHAR(20) NULL,
    [LastVisitDate] DATETIME2 NULL,
    [TriageCategory] NVARCHAR(100) NULL,
    [PatientId] INT NOT NULL,
    CONSTRAINT [PK_MedicalDatas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MedicalDatas_Patients_PatientId] FOREIGN KEY ([PatientId]) 
        REFERENCES [dbo].[Patients] ([Id]) ON DELETE CASCADE
);
GO

-- Create PatientIssues table
CREATE TABLE [dbo].[PatientIssues] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [PatientId] INT NOT NULL,
    [Title] NVARCHAR(200) NULL,
    [Description] NVARCHAR(2000) NULL,
    [CreatedAt] DATETIMEOFFSET NOT NULL,
    CONSTRAINT [PK_PatientIssues] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PatientIssues_Patients_PatientId] FOREIGN KEY ([PatientId]) 
        REFERENCES [dbo].[Patients] ([Id]) ON DELETE CASCADE
);
GO

-- Create indexes for better query performance
CREATE NONCLUSTERED INDEX [IX_MedicalDatas_PatientId] 
    ON [dbo].[MedicalDatas] ([PatientId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_PatientIssues_PatientId] 
    ON [dbo].[PatientIssues] ([PatientId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Patients_DomicileId] 
    ON [dbo].[Patients] ([DomicileId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Patients_PlaceOfBirthId] 
    ON [dbo].[Patients] ([PlaceOfBirthId] ASC);
GO

-- Verify all tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

PRINT 'Database schema created successfully!';
GO
```

**To execute this script:**

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open a new query window
4. Make sure you're connected to the `MedicalTriageDB` database (or run `USE MedicalTriageDB;` first)
5. Copy and paste the entire script above
6. Execute the script (Press F5 or click Execute)

**What this script creates:**
- `Domiciles` - Home address data
- `PlacesOfBirth` - Birth location data
- `Patients` - User accounts (patients and doctors) with foreign keys to Domiciles and PlacesOfBirth
- `MedicalDatas` - Medical information records with foreign key to Patients (CASCADE delete)
- `PatientIssues` - Patient-reported medical issues with foreign key to Patients (CASCADE delete)
- All necessary indexes for optimal query performance

**Note:** If you use the SQL script method, you should **NOT** run `dotnet ef database update` as it will try to apply migrations to an already-created database, which may cause conflicts.

**Optional: Drop and Recreate Database (if needed)**

If you need to completely reset the database (⚠️ **WARNING: This will delete all data**), you can use this script:

```sql
-- Drop all foreign key constraints first
USE MedicalTriageDB;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.MedicalDatas'))
    ALTER TABLE [dbo].[MedicalDatas] DROP CONSTRAINT [FK_MedicalDatas_Patients_PatientId];
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.PatientIssues'))
    ALTER TABLE [dbo].[PatientIssues] DROP CONSTRAINT [FK_PatientIssues_Patients_PatientId];
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.Patients'))
BEGIN
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Patients_Domiciles_DomicileId')
        ALTER TABLE [dbo].[Patients] DROP CONSTRAINT [FK_Patients_Domiciles_DomicileId];
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Patients_PlacesOfBirth_PlaceOfBirthId')
        ALTER TABLE [dbo].[Patients] DROP CONSTRAINT [FK_Patients_PlacesOfBirth_PlaceOfBirthId];
END
GO

-- Drop all tables
DROP TABLE IF EXISTS [dbo].[MedicalDatas];
DROP TABLE IF EXISTS [dbo].[PatientIssues];
DROP TABLE IF EXISTS [dbo].[Patients];
DROP TABLE IF EXISTS [dbo].[Domiciles];
DROP TABLE IF EXISTS [dbo].[PlacesOfBirth];
GO

PRINT 'All tables dropped. You can now run the creation script again.';
GO
```

### Step 4: Verify Database Setup

After creating the schema (using either method), verify the database was created successfully:

```sql
USE MedicalTriageDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
```

You should see all the tables listed:
- `Domiciles`
- `MedicalDatas`
- `PatientIssues`
- `Patients`
- `PlacesOfBirth`

You can also verify the foreign key relationships:

```sql
USE MedicalTriageDB;
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ParentTable,
    cp.name AS ParentColumn,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns AS cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
ORDER BY ParentTable, ForeignKeyName;
```

## Running the Application

1. **Navigate to the project directory**
   ```bash
   cd DigitalTriageApp
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

   Or use a specific profile:
   ```bash
   dotnet run --launch-profile https
   ```

3. **Open your browser** and navigate to:
   - HTTPS: `https://localhost:7266`
   - HTTP: `http://localhost:5180`

4. **First-time setup**: Create a new patient account using the registration page

## How the Application Works

### Authentication Flow

1. **Registration**: 
   - Users create an account with email and password
   - Password is hashed using BCrypt before storage
   - A default `MedicalData` record is created for new users
   - Users are assigned the "Patient" role by default

2. **Login**:
   - Users authenticate with email and password
   - Cookie-based session is created (expires after 8 hours)
   - Special doctor accounts: emails ending with `@hospital.com` with password `hospital` are treated as doctors (demo mode)

3. **Authorization**:
   - Role-based access control using ASP.NET Core Authorization
   - Patients can access their own data
   - Doctors can access the admin dashboard

### Application Structure

- **Razor Components**: Blazor Server components for interactive UI
- **Razor Pages**: Traditional Razor Pages for login (located in `Pages/Account/Login.cshtml`)
- **Services Layer**: Business logic separated into service classes:
  - `PatientService`: Patient CRUD operations and authentication
  - `MedicalDataService`: Medical data management
  - `PatientIssueService`: Issue tracking
- **Data Layer**: Entity Framework Core with `MedicalTriageDbContext`
- **Helpers**: Utility classes for authentication and antiforgery protection

### Key Features Implementation

1. **Personal Information Management**:
   - Patients can update personal details including CNP, citizenship, addresses
   - Place of birth and domicile are stored as separate entities

2. **Medical Information**:
   - One-to-many relationship: Patients can have multiple medical data records
   - Stores comprehensive medical history

3. **AI Triage**:
   - Currently in preview mode (UI placeholder)
   - Interface is ready for integration with AI/ML services

4. **Admin Dashboard**:
   - Available only to users with "Doctor" role
   - Provides comprehensive view of all patients and their medical records

## Project Structure

```
DigitalTriageApp/
├── Components/              # Blazor components
│   ├── Layout/             # Layout components
│   └── Pages/              # Shared page components
├── Controllers/            # API controllers (Antiforgery, Example)
├── Data/                   # Database context
│   └── MedicalTriageDbContext.cs
├── Helpers/                # Helper classes
│   ├── AntiforgeryHelper.cs
│   └── AuthHelper.cs
├── Migrations/             # EF Core migrations
├── Models/                 # Data models
│   ├── Patient.cs
│   ├── MedicalData.cs
│   ├── PatientIssue.cs
│   ├── PlaceOfBirth.cs
│   └── Domicile.cs
├── Pages/                  # Razor pages and Blazor pages
│   ├── Account/           # Login page (Razor Pages)
│   ├── Index.razor        # Home page
│   ├── Register.razor     # Registration
│   ├── Login.razor        # Blazor login (alternative)
│   ├── PersonalInfo.razor # Personal information management
│   ├── MedicalInfo.razor  # Medical data management
│   ├── AiChat.razor       # AI triage interface
│   └── AdminDashboard.razor # Admin dashboard
├── Services/               # Business logic services
│   ├── PatientService.cs
│   ├── MedicalDataService.cs
│   └── PatientIssueService.cs
├── Shared/                 # Shared components
│   └── NavMenu.razor      # Navigation menu
├── wwwroot/                # Static files (CSS, JS)
├── appsettings.json        # Production configuration
├── appsettings.Development.json # Development configuration
└── Program.cs              # Application entry point
```

## Configuration

### Application Settings

Key configuration in `appsettings.Development.json`:

- **ConnectionStrings**: Database connection string
- **Logging**: Log level configuration
- **BaseUrl**: API base URL (defaults to `https://localhost:7266`)

### Cookie Authentication Settings

Configured in `Program.cs`:
- **LoginPath**: `/Account/Login`
- **LogoutPath**: `/logout`
- **ExpireTimeSpan**: 8 hours
- **HttpOnly**: Enabled (security)
- **SameSite**: Lax mode
- **SecurePolicy**: SameAsRequest (Development), Always (Production)

### Antiforgery Protection

- Custom header: `X-CSRF-TOKEN`
- Cookie name: `__RequestVerificationToken`
- Protects against Cross-Site Request Forgery (CSRF) attacks

## User Roles

### Patient Role
- Default role for registered users
- Can access:
  - Personal information management
  - Medical information management
  - AI Triage (preview)
  - Patient issue submission

### Doctor Role
- Special role for healthcare professionals
- **Demo credentials**: Any email ending with `@hospital.com` and password `hospital`
- Can access:
  - All patient features
  - Admin Dashboard with full patient data access

## Troubleshooting

### Database Connection Issues

**Problem**: Cannot connect to database
- Verify SQL Server is running
- Check connection string in `appsettings.Development.json`
- Ensure database `MedicalTriageDB` exists
- Verify Windows Authentication or SQL Authentication credentials

**Solution**: Test connection using SQL Server Management Studio first

### Migration Errors

**Problem**: `dotnet ef database update` fails
- Ensure you have Entity Framework Core tools installed:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Verify connection string is correct
- Check if database exists and is accessible

### Authentication Issues

**Problem**: Cannot login after registration
- Check browser console for errors
- Verify cookies are enabled
- Clear browser cache and cookies
- Check that password hashing is working (verify BCrypt package is installed)

### Port Already in Use

**Problem**: Port 7266 or 5180 already in use
- Change ports in `Properties/launchSettings.json`
- Or kill the process using the port:
  ```bash
  # Windows
  netstat -ano | findstr :7266
  taskkill /PID <PID> /F
  ```

## Development Notes

- The application uses **Blazor Server** with Interactive Server rendering mode
- All components are server-side rendered with SignalR for real-time updates
- Password security: BCrypt hashing with automatic salt generation
- CSRF protection is enabled for all forms using antiforgery tokens
- Database migrations track schema changes - always run migrations after pulling updates

## Future Enhancements

- Full AI/ML integration for triage recommendations
- Email verification for new registrations
- Password reset functionality
- Real-time chat with healthcare professionals
- Appointment scheduling
- Prescription management
- Integration with external medical databases

