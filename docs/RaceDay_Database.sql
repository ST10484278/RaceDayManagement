-- Database creation and authentication tables (ROLE and USER)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RaceDay')
BEGIN
    CREATE DATABASE RaceDay;
END;
GO

USE RaceDay;
GO
-- 1. Drop foreign key dependencies from child tables if they exist
IF OBJECT_ID('dbo.RESULT', 'U') IS NOT NULL DROP TABLE dbo.RESULT;
IF OBJECT_ID('dbo.ENROLLMENT', 'U') IS NOT NULL DROP TABLE dbo.ENROLLMENT;
IF OBJECT_ID('dbo.EVENT_CATEGORY', 'U') IS NOT NULL DROP TABLE dbo.EVENT_CATEGORY;
IF OBJECT_ID('dbo.EVENT', 'U') IS NOT NULL DROP TABLE dbo.EVENT;

-- 2. Drop parent tables in reverse dependency order
IF OBJECT_ID('dbo.[USER]', 'U') IS NOT NULL DROP TABLE dbo.[USER];
IF OBJECT_ID('dbo.ROLE', 'U') IS NOT NULL DROP TABLE dbo.ROLE;
GO

CREATE TABLE dbo.ROLE (
    roleID INT IDENTITY(1,1) NOT NULL,
    roleName VARCHAR(50) NOT NULL,
    
    CONSTRAINT PK_ROLE PRIMARY KEY (roleID),
    CONSTRAINT UQ_ROLE_roleName UNIQUE (roleName)
);

CREATE TABLE dbo.[USER] (
    userID INT IDENTITY(1,1) NOT NULL,
    roleID INT NOT NULL,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    firstName VARCHAR(100) NOT NULL,
    lastName VARCHAR(100) NOT NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_USER PRIMARY KEY (userID),
    CONSTRAINT FK_USER_ROLE FOREIGN KEY (roleID) REFERENCES dbo.ROLE(roleID) ON DELETE CASCADE,
    CONSTRAINT UQ_USER_email UNIQUE (email)
);
GO
-- Create Event, Category, and Event_Category tables
USE RaceDay;
GO

-- Safe drop of dependent event tables
IF OBJECT_ID('dbo.EVENT_CATEGORY', 'U') IS NOT NULL DROP TABLE dbo.EVENT_CATEGORY;
IF OBJECT_ID('dbo.CATEGORY', 'U') IS NOT NULL DROP TABLE dbo.CATEGORY;
IF OBJECT_ID('dbo.EVENT', 'U') IS NOT NULL DROP TABLE dbo.EVENT;
GO

CREATE TABLE dbo.EVENT (
    eventID INT IDENTITY(1,1) NOT NULL,
    organiserID INT NOT NULL,
    eventName VARCHAR(150) NOT NULL,
    title VARCHAR(150) NOT NULL,
    description VARCHAR(MAX) NULL,
    eventDate DATETIME2 NOT NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_EVENT PRIMARY KEY (eventID),
    CONSTRAINT FK_EVENT_USER FOREIGN KEY (organiserID) REFERENCES dbo.[USER](userID)
);

CREATE TABLE dbo.CATEGORY (
    categoryID INT IDENTITY(1,1) NOT NULL,
    categoryName VARCHAR(100) NOT NULL,
    description VARCHAR(255) NULL,
    
    CONSTRAINT PK_CATEGORY PRIMARY KEY (categoryID),
    CONSTRAINT UQ_CATEGORY_categoryName UNIQUE (categoryName)
);

CREATE TABLE dbo.EVENT_CATEGORY (
    eventCategoryID INT IDENTITY(1,1) NOT NULL,
    categoryID INT NOT NULL,
    eventID INT NOT NULL,
    startLocation VARCHAR(255) NOT NULL,
    entryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    distance VARCHAR(50) NOT NULL,
    
    CONSTRAINT PK_EVENT_CATEGORY PRIMARY KEY (eventCategoryID),
    CONSTRAINT FK_EVENT_CATEGORY_CATEGORY FOREIGN KEY (categoryID) REFERENCES dbo.CATEGORY(categoryID),
    CONSTRAINT FK_EVENT_CATEGORY_EVENT FOREIGN KEY (eventID) REFERENCES dbo.EVENT(eventID) ON DELETE CASCADE
);
GO
-- Create Enrollment table linking participants to event categories
USE RaceDay;
GO

IF OBJECT_ID('dbo.ENROLLMENT', 'U') IS NOT NULL DROP TABLE dbo.ENROLLMENT;
GO

CREATE TABLE dbo.ENROLLMENT (
    enrollmentID INT IDENTITY(1,1) NOT NULL,
    participantID INT NOT NULL,
    eventCategoryID INT NOT NULL,
    enrollmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    enrollmentStatus VARCHAR(50) NOT NULL DEFAULT 'Confirmed',
    raceNumber INT NULL,
    
    CONSTRAINT PK_ENROLLMENT PRIMARY KEY (enrollmentID),
    CONSTRAINT FK_ENROLLMENT_USER FOREIGN KEY (participantID) REFERENCES dbo.[USER](userID),
    CONSTRAINT FK_ENROLLMENT_EVENT_CATEGORY FOREIGN KEY (eventCategoryID) REFERENCES dbo.EVENT_CATEGORY(eventCategoryID),
    CONSTRAINT UQ_ENROLLMENT_Participant_Category UNIQUE (participantID, eventCategoryID)
);
GO
-- Create Result table to track participant performance
USE RaceDay;
GO

IF OBJECT_ID('dbo.RESULT', 'U') IS NOT NULL DROP TABLE dbo.RESULT;
GO

CREATE TABLE dbo.RESULT (
    resultID INT IDENTITY(1,1) NOT NULL,
    enrollmentID INT NOT NULL,
    finishTime TIME(0) NULL,
    pace VARCHAR(20) NULL,
    position INT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Finished',
    
    CONSTRAINT PK_RESULT PRIMARY KEY (resultID),
    CONSTRAINT FK_RESULT_ENROLLMENT FOREIGN KEY (enrollmentID) REFERENCES dbo.ENROLLMENT(enrollmentID) ON DELETE CASCADE,
    CONSTRAINT UQ_RESULT_enrollmentID UNIQUE (enrollmentID)
);
GO
-- Seed initial roles and users
USE RaceDay;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ROLE WHERE roleName = 'Organiser')
    INSERT INTO dbo.ROLE (roleName) VALUES ('Organiser');

IF NOT EXISTS (SELECT 1 FROM dbo.ROLE WHERE roleName = 'Participant')
    INSERT INTO dbo.ROLE (roleName) VALUES ('Participant');

IF NOT EXISTS (SELECT 1 FROM dbo.[USER] WHERE email = 'organiser.soweto@raceday.co.za')
BEGIN
    INSERT INTO dbo.[USER] (roleID, email, password, firstName, lastName) 
    VALUES 
    (1, 'organiser.soweto@raceday.co.za', 'HashedPwd123!', 'Sipho', 'Ndlovu'),
    (1, 'organiser.capetown@raceday.co.za', 'HashedPwd456!', 'Anika', 'Van Der Merwe'),
    (2, 'john.doe@gmail.com', 'ParticipantPwd1!', 'John', 'Doe'),
    (2, 'thabo.mokoena@yahoo.com', 'ParticipantPwd2!', 'Thabo', 'Mokoena'),
    (2, 'sarah.smith@outlook.com', 'ParticipantPwd3!', 'Sarah', 'Smith');
END;
GO

-- Seed events, categories, and event categories
USE RaceDay;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.EVENT WHERE eventName = 'Soweto Marathon 2026')
BEGIN
    INSERT INTO dbo.EVENT (organiserID, eventName, title, description, eventDate) 
    VALUES 
    (1, 'Soweto Marathon 2026', 'The People''s Race', 'Annual road marathon through historical Soweto landmarks.', '2026-11-01 06:00:00'),
    (2, 'Cape Town Cycle Tour 2026', 'Rotary Cycle Challenge', 'Scenic coastal cycling event around the Cape Peninsula.', '2026-03-08 06:30:00');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.CATEGORY WHERE categoryName = 'Full Marathon')
BEGIN
    INSERT INTO dbo.CATEGORY (categoryName, description) 
    VALUES 
    ('Full Marathon', '42.2km long-distance road running race'),
    ('Half Marathon', '21.1km road race'),
    ('10km Road Race', '10km road running and walking event'),
    ('109km Road Cycle', 'Full perimeter cycling route');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.EVENT_CATEGORY WHERE eventID = 1 AND categoryID = 1)
BEGIN
    INSERT INTO dbo.EVENT_CATEGORY (eventID, categoryID, startLocation, entryFee, distance) 
    VALUES 
    (1, 1, 'Nasrec Expo Centre, Johannesburg', 350.00, '42.2 km'),
    (1, 2, 'Nasrec Expo Centre, Johannesburg', 250.00, '21.1 km'),
    (1, 3, 'Nasrec Expo Centre, Johannesburg', 150.00, '10.0 km'),
    (2, 4, 'Grand Parade, Cape Town', 550.00, '109.0 km');
END;
GO

-- Seed enrollments and results data
USE RaceDay;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ENROLLMENT WHERE participantID = 3 AND eventCategoryID = 1)
BEGIN
    INSERT INTO dbo.ENROLLMENT (participantID, eventCategoryID, enrollmentStatus, raceNumber) 
    VALUES 
    (3, 1, 'Confirmed', 1001),
    (4, 2, 'Confirmed', 2045),
    (5, 3, 'Confirmed', 3088),
    (3, 4, 'Confirmed', 5120);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.RESULT WHERE enrollmentID = 1)
BEGIN
    INSERT INTO dbo.RESULT (enrollmentID, finishTime, pace, position, status) 
    VALUES 
    (1, '03:45:12', '05:20 min/km', 142, 'Finished'),
    (2, '01:52:30', '05:20 min/km', 89, 'Finished'),
    (3, '00:55:10', '05:31 min/km', 45, 'Finished'),
    (4, NULL, NULL, NULL, 'DNS');
END;
GO