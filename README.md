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
- **Hospital Preference Management**: Search existing hospitals, filter by location, preview doctors assigned to each facility, or accept the automatically suggested closest hospital based on domicile
- **Personal Information Management**: Store and update personal details including CNP, citizenship, place of birth, and domicile address
- **Account Deletion**: Permanently delete account and all associated data with confirmation dialog (available in Personal Information page)
- **Medical Information Management**: Record and manage medical data including detailed anamnesis, triage information (ESI level, estimated wait time), privacy controls, and attached medical files
- **AI Triage Assistant**: Get preliminary medical triage recommendations based on symptoms (Preview mode - UI only)
- **Patient Issues Tracking**: Submit and track medical issues/concerns

### For Doctors/Administrators
- **Dedicated Doctor Registration**: Create doctor accounts with specialization details via `/register/doctor`
- **Hospital Management Workspace**: A dedicated `/hospital-management` page to create or update hospitals, search facilities, and manually join/leave memberships (creators are not joined automatically)
- **Admin Dashboard**: Access to comprehensive patient data and medical records filtered by the hospitals the doctor currently belongs to
- **Patient Management**: View all registered patients and their information
- **Medical Records Access**: Full access to patient medical histories, including confidential flags and authorized-doctor relationships
- **Streamlined Navigation**: Role-based menu visibility - Profile section hidden, only Administration section visible (Admin Dashboard + Hospital Management)

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

> ⚠️ **Prefer Entity Framework migrations.** Only use the script below if you must create the schema manually. After running it, insert migration rows manually into `__EFMigrationsHistory` so EF doesn’t try to recreate the tables.

**Complete SQL Script (schema current as of the latest update):**

```sql
USE MedicalTriageDB;
GO

/* Drop existing tables if you are rebuilding */
IF OBJECT_ID('dbo.MedicalFiles','U') IS NOT NULL DROP TABLE dbo.MedicalFiles;
IF OBJECT_ID('dbo.DoctorHospitalMemberships','U') IS NOT NULL DROP TABLE dbo.DoctorHospitalMemberships;
IF OBJECT_ID('dbo.MedicalDatas','U') IS NOT NULL DROP TABLE dbo.MedicalDatas;
IF OBJECT_ID('dbo.PatientIssues','U') IS NOT NULL DROP TABLE dbo.PatientIssues;
IF OBJECT_ID('dbo.Hospitals','U') IS NOT NULL DROP TABLE dbo.Hospitals;
IF OBJECT_ID('dbo.DoctorProfiles','U') IS NOT NULL DROP TABLE dbo.DoctorProfiles;
IF OBJECT_ID('dbo.Patients','U') IS NOT NULL DROP TABLE dbo.Patients;
IF OBJECT_ID('dbo.Domiciles','U') IS NOT NULL DROP TABLE dbo.Domiciles;
IF OBJECT_ID('dbo.PlacesOfBirth','U') IS NOT NULL DROP TABLE dbo.PlacesOfBirth;

/* Base tables */
CREATE TABLE dbo.PlacesOfBirth
(
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Country  NVARCHAR(100) NULL,
    County   NVARCHAR(100) NULL,
    City     NVARCHAR(100) NULL
);

CREATE TABLE dbo.Domiciles
(
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Country  NVARCHAR(100) NULL,
    County   NVARCHAR(100) NULL,
    City     NVARCHAR(100) NULL,
    Street   NVARCHAR(150) NULL,
    Number   NVARCHAR(20)  NULL
);

CREATE TABLE dbo.Patients
(
    Id                   INT IDENTITY(1,1) PRIMARY KEY,
    Email                NVARCHAR(200) NOT NULL,
    PasswordHash         NVARCHAR(MAX) NOT NULL,
    PhoneNumber          NVARCHAR(20) NULL,
    FirstName            NVARCHAR(100) NULL,
    LastName             NVARCHAR(100) NULL,
    Cnp                  NVARCHAR(13) NULL,
    Serie                NVARCHAR(2)  NULL,
    Nr                   NVARCHAR(6)  NULL,
    Citizenship          NVARCHAR(100) NULL,
    PlaceOfBirthId       INT NULL,
    DomicileId           INT NULL,
    Role                 NVARCHAR(20) NULL,
    PreferredHospitalId  INT NULL
);

CREATE TABLE dbo.DoctorProfiles
(
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL,
    Specialization NVARCHAR(150) NOT NULL
);

CREATE TABLE dbo.Hospitals
(
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    Name               NVARCHAR(200) NOT NULL,
    Country            NVARCHAR(100) NULL,
    County             NVARCHAR(100) NULL,
    City               NVARCHAR(100) NULL,
    Street             NVARCHAR(150) NULL,
    Number             NVARCHAR(20)  NULL,
    CreatedByDoctorId  INT NULL
);

CREATE TABLE dbo.DoctorHospitalMemberships
(
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    DoctorId    INT NOT NULL,
    HospitalId  INT NOT NULL,
    JoinedAt    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LeftAt      DATETIME2 NULL,
    IsActive    BIT NOT NULL DEFAULT(1)
);

CREATE TABLE dbo.MedicalDatas
(
    Id                        INT IDENTITY(1,1) PRIMARY KEY,
    BloodType                 NVARCHAR(5) NULL,
    Allergies                 NVARCHAR(1000) NULL,
    ChronicDiseases           NVARCHAR(1000) NULL,
    CurrentMedication         NVARCHAR(1000) NULL,
    PersonalHistory           NVARCHAR(1000) NULL,
    FamilyHistory             NVARCHAR(1000) NULL,
    LivingConditions          NVARCHAR(500) NULL,
    IncidentLocation          NVARCHAR(200) NULL,
    Symptoms                  NVARCHAR(2000) NULL,
    PreliminaryDiagnosis      NVARCHAR(2000) NULL,
    EmergencyContactName      NVARCHAR(200) NULL,
    EmergencyContactPhone     NVARCHAR(20) NULL,
    LastVisitDate             DATETIME2 NULL,
    TriageCategory            NVARCHAR(100) NULL,
    TriageLevel               INT NULL,
    EstimatedWaitTimeMinutes  INT NULL,
    IsConfidential            BIT NOT NULL DEFAULT(1),
    AuthorizedDoctorId        INT NULL,
    CreatedAt                 DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt                 DATETIME2 NULL,
    PatientId                 INT NOT NULL
);

CREATE TABLE dbo.MedicalFiles
(
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    MedicalDataId  INT NOT NULL,
    FileName       NVARCHAR(255) NOT NULL,
    FilePath       NVARCHAR(255) NOT NULL,
    UploadDate     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.PatientIssues
(
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    PatientId   INT NOT NULL,
    Title       NVARCHAR(200) NULL,
    Description NVARCHAR(2000) NULL,
    CreatedAt   DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

/* Foreign keys */
ALTER TABLE dbo.Patients  ADD CONSTRAINT FK_Patients_PlacesOfBirth FOREIGN KEY (PlaceOfBirthId) REFERENCES dbo.PlacesOfBirth(Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Patients  ADD CONSTRAINT FK_Patients_Domiciles     FOREIGN KEY (DomicileId) REFERENCES dbo.Domiciles(Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Patients  ADD CONSTRAINT FK_Patients_Hospitals     FOREIGN KEY (PreferredHospitalId) REFERENCES dbo.Hospitals(Id) ON DELETE SET NULL;

ALTER TABLE dbo.DoctorProfiles ADD CONSTRAINT FK_DoctorProfiles_Patients FOREIGN KEY (UserId) REFERENCES dbo.Patients(Id) ON DELETE CASCADE;
ALTER TABLE dbo.Hospitals ADD CONSTRAINT FK_Hospitals_DoctorProfiles FOREIGN KEY (CreatedByDoctorId) REFERENCES dbo.DoctorProfiles(Id) ON DELETE SET NULL;
ALTER TABLE dbo.DoctorHospitalMemberships ADD CONSTRAINT FK_DoctorHospitalMemberships_Doctors   FOREIGN KEY (DoctorId)   REFERENCES dbo.DoctorProfiles(Id) ON DELETE CASCADE;
ALTER TABLE dbo.DoctorHospitalMemberships ADD CONSTRAINT FK_DoctorHospitalMemberships_Hospitals FOREIGN KEY (HospitalId) REFERENCES dbo.Hospitals(Id)       ON DELETE CASCADE;
ALTER TABLE dbo.MedicalDatas ADD CONSTRAINT FK_MedicalDatas_Patients          FOREIGN KEY (PatientId)          REFERENCES dbo.Patients(Id)        ON DELETE CASCADE;
ALTER TABLE dbo.MedicalDatas ADD CONSTRAINT FK_MedicalDatas_DoctorProfiles    FOREIGN KEY (AuthorizedDoctorId) REFERENCES dbo.DoctorProfiles(Id) ON DELETE SET NULL;
ALTER TABLE dbo.MedicalFiles ADD CONSTRAINT FK_MedicalFiles_MedicalDatas FOREIGN KEY (MedicalDataId) REFERENCES dbo.MedicalDatas(Id) ON DELETE CASCADE;
ALTER TABLE dbo.PatientIssues ADD CONSTRAINT FK_PatientIssues_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients(Id) ON DELETE CASCADE;

/* Indexes */
CREATE INDEX IX_Patients_DomicileId        ON dbo.Patients(DomicileId);
CREATE INDEX IX_Patients_PlaceOfBirthId    ON dbo.Patients(PlaceOfBirthId);
CREATE INDEX IX_Patients_PreferredHospital ON dbo.Patients(PreferredHospitalId);
CREATE INDEX IX_DoctorProfiles_UserId      ON dbo.DoctorProfiles(UserId);
CREATE INDEX IX_Hospitals_CreatedByDoctor  ON dbo.Hospitals(CreatedByDoctorId);
CREATE UNIQUE INDEX IX_DoctorHospitalMemberships_Doctor_Hospital_IsActive ON dbo.DoctorHospitalMemberships(DoctorId, HospitalId, IsActive);
CREATE INDEX IX_MedicalDatas_PatientId        ON dbo.MedicalDatas(PatientId);
CREATE INDEX IX_MedicalDatas_AuthorizedDoctor ON dbo.MedicalDatas(AuthorizedDoctorId);
CREATE INDEX IX_MedicalFiles_MedicalDataId    ON dbo.MedicalFiles(MedicalDataId);
CREATE INDEX IX_PatientIssues_PatientId       ON dbo.PatientIssues(PatientId);
GO

/* Optionally mark migrations as applied */
IF OBJECT_ID('__EFMigrationsHistory','U') IS NULL
BEGIN
    CREATE TABLE __EFMigrationsHistory (MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY, ProductVersion NVARCHAR(32) NOT NULL);
END;
MERGE __EFMigrationsHistory AS target
USING (VALUES
    ('20251102161050_InitialCreate','9.0.10'),
    ('20251110154456_AddHospitalsAndDoctorProfiles','9.0.10'),
    ('20251110172353_UpdateMedicalDataModel','9.0.10')
) AS source(MigrationId, ProductVersion)
ON target.MigrationId = source.MigrationId
WHEN NOT MATCHED THEN INSERT (MigrationId, ProductVersion) VALUES (source.MigrationId, source.ProductVersion);
GO
```

**What this script creates:**
- Full hospital/doctor infrastructure (including membership table with cascade deletes)
- Extended `MedicalDatas` schema with anamnesis, triage (ESI) and privacy fields plus authorized doctor linkage
- `MedicalFiles` table for attachments (cascade delete per medical record)
- Role-based patient schema used by the current application
- All necessary indexes

After running the script, EF Core believes all migrations are applied thanks to the final MERGE statement. You can disable that part if you prefer to manage migration history manually.

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

4. **First-time setup**: Create a new patient account at `/register`, or create a doctor account with specialization details at `/register/doctor`

## How the Application Works

### Authentication Flow

1. **Registration**: 
   - Patients sign up via `/register`; doctors sign up via `/register/doctor` and provide their medical specialization
   - Passwords are hashed using BCrypt before storage
   - A default `MedicalData` record is created for every new account
   - Patients automatically receive the "Patient" role; doctors created through the doctor registration flow receive the "Doctor" role
   - Patients without a preferred hospital are auto-assigned to the closest matching hospital based on domicile (if available)

2. **Login**:
   - Users authenticate with email and password
   - Cookie-based session is created (expires after 8 hours)
   - Doctor accounts use the same login endpoint after they have been created through the doctor registration flow

3. **Authorization**:
   - Role-based access control using ASP.NET Core Authorization
   - Patients can access their own data
   - Doctors can access the admin dashboard

### Application Structure

- **Razor Components**: Blazor Server components for interactive UI
- **Razor Pages**: Traditional Razor Pages for login (located in `Pages/Account/Login.cshtml`)
- **Services Layer**: Business logic separated into service classes:
  - `PatientService`: Patient CRUD operations and authentication
  - `HospitalService`: Hospital creation, membership management, and patient hospital assignment
  - `MedicalDataService`: Medical data management
  - `PatientIssueService`: Issue tracking
- **Data Layer**: Entity Framework Core with `MedicalTriageDbContext`
- **Helpers**: Utility classes for authentication and antiforgery protection

### Key Features Implementation

1. **Personal Information Management**:
   - Patients can update personal details including CNP, citizenship, addresses
   - Place of birth and domicile are stored as separate entities
   - **Account Deletion**: Patients can permanently delete their account with a confirmation dialog
   - Deletion removes all associated data: personal info, medical data, reported issues, and related addresses (if not shared)

2. **Medical Information**:
   - One-to-many relationship: Patients can have multiple medical data records
   - Stores comprehensive medical history

3. **AI Triage**:
   - Currently in preview mode (UI placeholder)
   - Interface is ready for integration with AI/ML services

4. **Admin Dashboard**:
   - Available only to users with "Doctor" role
   - Provides a comprehensive view of patient medical records filtered by hospitals the doctor currently belongs to

5. **Hospital & Doctor Collaboration**:
   - Doctors manage hospitals on a dedicated `/hospital-management` page, manually joining/leaving facilities (creators are not auto-joined)
   - Hospitals with no active doctors are automatically deleted to prevent orphaned facilities
   - Patients can search existing hospitals, preview assigned doctors, and select their preferred facility

6. **User Interface Enhancements**:
   - **Fixed Sidebar Navigation**: Navigation menu remains visible while scrolling page content
   - **Role-Based Menu**: Profile section (Personal Information, Medical Information, AI Triage) only visible to patients
   - Doctors see a streamlined menu with only Home, Admin Dashboard, and Exit options

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
│   ├── DoctorProfile.cs
│   ├── DoctorHospitalMembership.cs
│   ├── Hospital.cs
│   ├── MedicalData.cs
│   ├── PatientIssue.cs
│   ├── PlaceOfBirth.cs
│   └── Domicile.cs
├── Pages/                  # Razor pages and Blazor pages
│   ├── Account/           # Login page (Razor Pages)
│   ├── Index.razor        # Home page
│   ├── Register.razor     # Registration
│   ├── RegisterDoctor.razor # Doctor-specific registration
│   ├── Login.razor        # Blazor login (alternative)
│   ├── PersonalInfo.razor # Personal information management
│   ├── MedicalInfo.razor  # Medical data management
│   ├── AiChat.razor       # AI triage interface
│   ├── AdminDashboard.razor # Doctor dashboard (patient insights)
│   └── HospitalManagement.razor # Doctor-only hospital management workspace
├── Services/               # Business logic services
│   ├── PatientService.cs
│   ├── HospitalService.cs
│   ├── MedicalDataService.cs
│   ├── PatientIssueService.cs
│   ├── IPatientService.cs
│   ├── IHospitalService.cs
│   ├── IMedicalDataService.cs
│   └── IPatientIssueService.cs
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