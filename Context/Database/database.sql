-- =============================================
-- CREATE DATABASE
-- =============================================
USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'CustomerManagementDB')
BEGIN
    ALTER DATABASE CustomerManagementDB 
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CustomerManagementDB;
END
GO

-- Create new database
CREATE DATABASE CustomerManagementDB;
GO

-- Switch to new database
USE CustomerManagementDB;
GO

-- =============================================
-- CREATE SEGMENT TABLE
-- =============================================
CREATE TABLE Segment(
    SegmentId INT IDENTITY(1,1) PRIMARY KEY,
    SegmentName VARCHAR(100) NOT NULL,
    Description VARCHAR(200)
);
GO

-- =============================================
-- CREATE CUSTOMER TABLE
-- =============================================
CREATE TABLE Customer(
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) UNIQUE,
    Phone VARCHAR(20),
    Website VARCHAR(200),
    Industry VARCHAR(100),
    CompanySize VARCHAR(50),
    Classification VARCHAR(50) CHECK (Classification IN ('Prospect','Active','Inactive','VIP','At-Risk')),
    Type VARCHAR(50) CHECK (Type IN ('Business','Individual')),
    SegmentId INT,
    ParentCustomerId INT NULL,
    AccountValue DECIMAL(18,2) DEFAULT 0,
    HealthScore INT DEFAULT 100,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0
);
GO

-- =============================================
-- CREATE ORDERS TABLE
-- =============================================
CREATE TABLE Orders(
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    ProductName VARCHAR(150),
    Quantity INT,
    TotalAmount DECIMAL(10,2),
    CustomerId INT,
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId)
);
GO

-- =============================================
-- CREATE CONTACT PERSON TABLE
-- =============================================
CREATE TABLE ContactPerson(
    ContactPersonId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    Name VARCHAR(150),
    Email VARCHAR(150),
    Phone VARCHAR(20),
    Title VARCHAR(100),
    IsPrimary BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId) ON DELETE CASCADE
);
GO

-- =============================================
-- CREATE CUSTOMER ADDRESS TABLE
-- =============================================
CREATE TABLE CustomerAddress(
    AddressId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    AddressType VARCHAR(50) CHECK (AddressType IN ('Billing','Shipping','Primary')),
    Street VARCHAR(200) NOT NULL,
    City VARCHAR(100) NOT NULL,
    State VARCHAR(100) NOT NULL,
    PostalCode VARCHAR(20) NOT NULL,
    Country VARCHAR(100) NOT NULL,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId) ON DELETE CASCADE
);
GO

ALTER TABLE CustomerAddress
ADD CONSTRAINT chk_PostalCode CHECK (LEN(PostalCode) >= 5);
GO

-- =============================================
-- CREATE CUSTOMER INTERACTION TABLE
-- =============================================
CREATE TABLE CustomerInteraction(
    InteractionId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    InteractionType VARCHAR(50) CHECK (InteractionType IN ('Call','Email','Meeting','Support Ticket')),
    Subject VARCHAR(200),
    Details VARCHAR(MAX),
    InteractionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId) ON DELETE CASCADE
);
GO

-- =============================================
-- CREATE CUSTOMER AUDIT TABLE
-- =============================================
CREATE TABLE CustomerAudit(
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT,
    ChangedField VARCHAR(100),
    OldValue VARCHAR(200),
    NewValue VARCHAR(200),
    ChangedDate DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- TRIGGERS
-- =============================================
-- Soft Delete Trigger
CREATE TRIGGER trg_SoftDeleteCustomer
ON Customer
INSTEAD OF DELETE
AS
BEGIN
    UPDATE Customer
    SET IsDeleted = 1
    WHERE CustomerId IN (SELECT CustomerId FROM deleted);
END;
GO

-- Audit Trigger
CREATE TRIGGER trg_CustomerAudit
ON Customer
AFTER UPDATE
AS
BEGIN
    INSERT INTO CustomerAudit (CustomerId, ChangedField, OldValue, NewValue)
    SELECT
        i.CustomerId,
        'AccountValue',
        CAST(d.AccountValue AS VARCHAR),
        CAST(i.AccountValue AS VARCHAR)
    FROM inserted i
    JOIN deleted d ON i.CustomerId = d.CustomerId
    WHERE i.AccountValue <> d.AccountValue;
END;
GO

-- Modified Date Update Trigger
CREATE TRIGGER trg_UpdateCustomerModifiedDate
ON Customer
AFTER UPDATE
AS
BEGIN
    UPDATE Customer
    SET ModifiedDate = GETDATE()
    FROM Customer c
    INNER JOIN inserted i ON c.CustomerId = i.CustomerId;
END;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================
CREATE PROCEDURE sp_CustomerLifetimeValue
AS
BEGIN
    SELECT c.CustomerId, c.CustomerName, SUM(o.TotalAmount) AS LifetimeValue
    FROM Customer c
    LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
    GROUP BY c.CustomerId, c.CustomerName;
END;
GO

CREATE PROCEDURE sp_CustomerHealthScore
AS
BEGIN
    SELECT c.CustomerId, c.CustomerName, COUNT(i.InteractionId) AS TotalInteractions,
           SUM(o.TotalAmount) AS TotalBusiness,
           CASE
               WHEN SUM(o.TotalAmount) > 200000 THEN 90
               WHEN SUM(o.TotalAmount) > 100000 THEN 70
               ELSE 50
           END AS HealthScore
    FROM Customer c
    LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
    LEFT JOIN CustomerInteraction i ON c.CustomerId = i.CustomerId
    GROUP BY c.CustomerId, c.CustomerName;
END;
GO

CREATE PROCEDURE sp_DuplicateCustomers
AS
BEGIN
    SELECT Email, COUNT(*) AS DuplicateCount
    FROM Customer
    GROUP BY Email
    HAVING COUNT(*) > 1;
END;
GO

-- =============================================
-- INDEXES
-- =============================================
CREATE INDEX idx_customer_email ON Customer(Email);
CREATE INDEX idx_orders_customer ON Orders(CustomerId);
CREATE INDEX idx_interaction_customer ON CustomerInteraction(CustomerId);
GO

-- =============================================
-- INSERT SAMPLE DATA
-- =============================================

-- Segments
INSERT INTO Segment (SegmentName, Description)
VALUES 
('Enterprise','Large Enterprise Customers'),
('SMB','Small Medium Business');
GO

-- Customers
INSERT INTO Customer
(CustomerName, Email, Phone, Website, Industry, CompanySize, Classification, Type, SegmentId, AccountValue)
VALUES
('Revature','info@revature.com','9000000001','www.revature.com','IT Services','500+','Active','Business',1,500000),
('Prajakta Consulting','prajakta@gmail.com','9000000002','www.prajaktaconsulting.com','Consulting','10-50','Prospect','Business',2,50000),
('Tanaya Solutions','tanaya@gmail.com','9000000003','www.tanayasolutions.com','IT Services','100-200','Active','Business',1,200000),
('GlobalTech','contact@globaltech.com','9000000004','www.globaltech.com','Technology','200-500','VIP','Business',1,750000),
('Bright Minds Academy','info@brightminds.com','9000000005','www.brightminds.com','Education','50-100','Active','Business',2,120000);
GO

-- Contact Persons
INSERT INTO ContactPerson (CustomerId, Name, Email, Phone, Title, IsPrimary)
VALUES
(1, 'John Doe', 'john.doe@revature.com', '9000000010', 'Manager', 1),
(2, 'Priya Sharma', 'priya.sharma@prajaktaconsulting.com', '9000000020', 'Lead Consultant', 1),
(3, 'Vishal Mehta', 'vishal.mehta@tanayasolutions.com', '9000000030', 'CEO', 1),
(1, 'Alice Johnson', 'alice.johnson@revature.com', '9000000040', 'Team Lead', 0),
(2, 'Rohit Patil', 'rohit.patil@prajaktaconsulting.com', '9000000050', 'Consultant', 0),
(3, 'Kiran Mehta', 'kiran.mehta@tanayasolutions.com', '9000000060', 'CTO', 0),
(4, 'Sarah Lee', 'sarah.lee@globaltech.com', '9000000070', 'CEO', 1),
(4, 'David Kim', 'david.kim@globaltech.com', '9000000080', 'CFO', 0),
(5, 'Anita Desai', 'anita.desai@brightminds.com', '9000000090', 'Director', 1),
(5, 'Ramesh Patil', 'ramesh.patil@brightminds.com', '9000000100', 'Teacher', 0);
GO

-- Customer Addresses
INSERT INTO CustomerAddress (CustomerId, AddressType, Street, City, State, PostalCode, Country)
VALUES
(1, 'Primary', '123 Tech Park', 'Pune', 'Maharashtra', '411001', 'India'),
(1, 'Billing', '456 Business Rd', 'Pune', 'Maharashtra', '411002', 'India'),
(2, 'Primary', '789 Consulting Ave', 'Mumbai', 'Maharashtra', '400001', 'India'),
(3, 'Primary', '101 Solutions Blvd', 'Bangalore', 'Karnataka', '560001', 'India'),
(4, 'Primary', '22 Global St', 'Hyderabad', 'Telangana', '500081', 'India'),
(4, 'Billing', '45 Techway', 'Hyderabad', 'Telangana', '500082', 'India'),
(5, 'Primary', '10 Bright Rd', 'Delhi', 'Delhi', '110001', 'India');
GO

-- Customer Interactions
INSERT INTO CustomerInteraction (CustomerId, InteractionType, Subject, Details)
VALUES
(1, 'Call', 'Project Discussion', 'Discussed upcoming project requirements'),
(1, 'Email', 'Invoice Sent', 'Sent invoice for last month'),
(2, 'Meeting', 'Consulting Proposal', 'Presented consulting proposal to client'),
(3, 'Support Ticket', 'Software Issue', 'Resolved issue with software license activation'),
(4, 'Email', 'Contract Signed', 'Signed contract for new product implementation'),
(5, 'Meeting', 'Curriculum Review', 'Reviewed curriculum with teachers'),
(5, 'Support Ticket', 'Portal Issue', 'Resolved login issues for students');
GO

-- Orders
INSERT INTO Orders (ProductName, Quantity, TotalAmount, CustomerId)
VALUES
('Laptop', 2, 150000, 1),
('Consulting Package', 1, 50000, 2),
('Software License', 5, 200000, 3),
('Cloud Subscription', 3, 300000, 4),
('Education Software', 10, 50000, 5);
GO