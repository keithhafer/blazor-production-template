-- =============================================
-- Blazor Production App - Database Schema
-- SQL Server / Azure SQL Database
-- =============================================

-- Create Database (commented out for production)
-- CREATE DATABASE BlazorAppDb;
-- GO

USE BlazorAppDb;
GO

-- =============================================
-- Products Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [StockQuantity] INT NOT NULL DEFAULT 0,
        [Category] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0),
        CONSTRAINT [CK_Products_StockQuantity] CHECK ([StockQuantity] >= 0)
    );

    -- Indexes for performance
    CREATE NONCLUSTERED INDEX [IX_Products_Name] ON [dbo].[Products] ([Name]);
    CREATE NONCLUSTERED INDEX [IX_Products_Category] ON [dbo].[Products] ([Category]);
    CREATE NONCLUSTERED INDEX [IX_Products_IsActive] ON [dbo].[Products] ([IsActive]);
END
GO

-- =============================================
-- Sample Data
-- =============================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products])
BEGIN
    INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [StockQuantity], [Category], [IsActive])
    VALUES 
        ('Laptop Pro 15"', 'High-performance laptop with 16GB RAM and 512GB SSD', 1299.99, 25, 'Electronics', 1),
        ('Wireless Mouse', 'Ergonomic wireless mouse with USB receiver', 29.99, 150, 'Accessories', 1),
        ('USB-C Hub', '7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader', 49.99, 75, 'Accessories', 1),
        ('Monitor 27"', '4K UHD monitor with HDR support', 399.99, 30, 'Electronics', 1),
        ('Mechanical Keyboard', 'RGB mechanical gaming keyboard with brown switches', 89.99, 60, 'Accessories', 1),
        ('Webcam HD', '1080p HD webcam with noise-canceling microphone', 79.99, 45, 'Electronics', 1),
        ('Laptop Stand', 'Aluminum laptop stand with adjustable height', 39.99, 100, 'Accessories', 1),
        ('External SSD 1TB', 'Portable SSD with USB 3.2 Gen 2 connectivity', 149.99, 50, 'Storage', 1),
        ('Bluetooth Headphones', 'Active noise cancellation wireless headphones', 199.99, 40, 'Audio', 1),
        ('Desk Lamp LED', 'Adjustable LED desk lamp with USB charging port', 34.99, 80, 'Office', 1);
    
    PRINT 'Sample data inserted successfully';
END
GO

-- =============================================
-- Stored Procedures (Optional - Dapper can use raw SQL or SPs)
-- =============================================

-- Get all active products
CREATE OR ALTER PROCEDURE [dbo].[sp_GetActiveProducts]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Name, Description, Price, StockQuantity, 
        Category, CreatedAt, UpdatedAt, IsActive
    FROM [dbo].[Products]
    WHERE IsActive = 1
    ORDER BY Name;
END
GO

-- Search products by term
CREATE OR ALTER PROCEDURE [dbo].[sp_SearchProducts]
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Name, Description, Price, StockQuantity, 
        Category, CreatedAt, UpdatedAt, IsActive
    FROM [dbo].[Products]
    WHERE IsActive = 1
        AND (Name LIKE '%' + @SearchTerm + '%' 
            OR Description LIKE '%' + @SearchTerm + '%'
            OR Category LIKE '%' + @SearchTerm + '%')
    ORDER BY Name;
END
GO

-- =============================================
-- Database Information
-- =============================================
PRINT '==========================================';
PRINT 'Database Schema Created Successfully';
PRINT '==========================================';
PRINT 'Tables Created: Products';
PRINT 'Sample Records: 10 products';
PRINT 'Stored Procedures: sp_GetActiveProducts, sp_SearchProducts';
PRINT '==========================================';
