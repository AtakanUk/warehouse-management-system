-- =============================================================================
-- Warehouse Management System - database schema (SQL Server 2016 or newer)
--
-- The application provisions an empty database from this file on first start.
-- To create it manually instead:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "CREATE DATABASE WarehouseDB"
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d WarehouseDB -i database\schema.sql
--
-- Business rules are enforced in the domain model and, as a safety net, by the
-- constraints below. ROWVERSION columns provide optimistic concurrency control.
-- =============================================================================

CREATE TABLE dbo.Products
(
    Id               INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Sku              NVARCHAR(32)       NOT NULL CONSTRAINT UQ_Products_Sku UNIQUE,
    Name             NVARCHAR(100)      NOT NULL,
    Description      NVARCHAR(500)      NULL,
    WeightKg         DECIMAL(10, 3)     NULL CONSTRAINT CK_Products_WeightKg CHECK (WeightKg >= 0),
    Dimensions       NVARCHAR(50)       NULL,
    CountryOfOrigin  CHAR(2)            NULL,
    ReorderLevel     INT                NOT NULL CONSTRAINT DF_Products_ReorderLevel DEFAULT (0),
    QuantityOnHand   INT                NOT NULL CONSTRAINT DF_Products_QuantityOnHand DEFAULT (0),
    QuantityReserved INT                NOT NULL CONSTRAINT DF_Products_QuantityReserved DEFAULT (0),
    RowVersion       ROWVERSION         NOT NULL,
    CONSTRAINT CK_Products_ReorderLevel CHECK (ReorderLevel >= 0),
    -- Reserved stock is part of the stock on hand; neither can go negative.
    CONSTRAINT CK_Products_Quantities CHECK (QuantityReserved >= 0 AND QuantityReserved <= QuantityOnHand)
);

CREATE TABLE dbo.Customers
(
    Id          INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    Name        NVARCHAR(100)      NOT NULL,
    ContactName NVARCHAR(100)      NULL,
    Email       NVARCHAR(254)      NULL,
    Phone       NVARCHAR(32)       NULL,
    Address     NVARCHAR(250)      NULL
);

CREATE TABLE dbo.Carriers
(
    Id          INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Carriers PRIMARY KEY,
    Name        NVARCHAR(100)      NOT NULL CONSTRAINT UQ_Carriers_Name UNIQUE,
    ContactName NVARCHAR(100)      NULL,
    Email       NVARCHAR(254)      NULL,
    Phone       NVARCHAR(32)       NULL,
    Address     NVARCHAR(250)      NULL
);

-- Status: 0 = Open, 1 = Picked (stock reserved), 2 = Shipped, 3 = Cancelled
CREATE TABLE dbo.Orders
(
    Id                INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    CustomerId        INT                NOT NULL CONSTRAINT FK_Orders_Customers REFERENCES dbo.Customers (Id),
    Status            TINYINT            NOT NULL CONSTRAINT DF_Orders_Status DEFAULT (0),
    CustomerReference NVARCHAR(50)       NULL,
    CreatedAtUtc      DATETIME2(0)       NOT NULL,
    CreatedBy         NVARCHAR(50)       NOT NULL,
    PickedAtUtc       DATETIME2(0)       NULL,
    PickedBy          NVARCHAR(50)       NULL,
    ShippedAtUtc      DATETIME2(0)       NULL,
    ShippedBy         NVARCHAR(50)       NULL,
    CancelledAtUtc    DATETIME2(0)       NULL,
    CancelledBy       NVARCHAR(50)       NULL,
    CarrierId         INT                NULL CONSTRAINT FK_Orders_Carriers REFERENCES dbo.Carriers (Id),
    TrackingNumber    NVARCHAR(50)       NULL,
    RowVersion        ROWVERSION         NOT NULL,
    CONSTRAINT CK_Orders_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_Orders_Picked CHECK (Status NOT IN (1, 2) OR PickedAtUtc IS NOT NULL),
    CONSTRAINT CK_Orders_Shipped CHECK (Status <> 2 OR (ShippedAtUtc IS NOT NULL AND CarrierId IS NOT NULL))
);

CREATE INDEX IX_Orders_Status ON dbo.Orders (Status, CreatedAtUtc);
CREATE INDEX IX_Orders_CustomerId ON dbo.Orders (CustomerId);
CREATE INDEX IX_Orders_CarrierId ON dbo.Orders (CarrierId);

CREATE TABLE dbo.OrderLines
(
    Id        INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_OrderLines PRIMARY KEY,
    OrderId   INT                NOT NULL CONSTRAINT FK_OrderLines_Orders REFERENCES dbo.Orders (Id) ON DELETE CASCADE,
    ProductId INT                NOT NULL CONSTRAINT FK_OrderLines_Products REFERENCES dbo.Products (Id),
    Quantity  INT                NOT NULL CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT UQ_OrderLines_Order_Product UNIQUE (OrderId, ProductId)
);

CREATE INDEX IX_OrderLines_ProductId ON dbo.OrderLines (ProductId);

-- Inventory ledger: QuantityOnHand of a product equals the sum of its movements.
-- Type: 1 = Receipt (positive quantity), 2 = Shipment (negative quantity)
CREATE TABLE dbo.StockMovements
(
    Id           INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_StockMovements PRIMARY KEY,
    ProductId    INT                NOT NULL CONSTRAINT FK_StockMovements_Products REFERENCES dbo.Products (Id),
    Type         TINYINT            NOT NULL,
    Quantity     INT                NOT NULL,
    Reference    NVARCHAR(50)       NULL,
    OrderId      INT                NULL CONSTRAINT FK_StockMovements_Orders REFERENCES dbo.Orders (Id),
    CreatedAtUtc DATETIME2(0)       NOT NULL,
    CreatedBy    NVARCHAR(50)       NOT NULL,
    CONSTRAINT CK_StockMovements_Type CHECK ((Type = 1 AND Quantity > 0) OR (Type = 2 AND Quantity < 0))
);

CREATE INDEX IX_StockMovements_Product_Created ON dbo.StockMovements (ProductId, CreatedAtUtc DESC);
CREATE INDEX IX_StockMovements_Created ON dbo.StockMovements (CreatedAtUtc DESC);

-- PasswordHash format: PBKDF2-SHA512$<iterations>$<base64 salt>$<base64 key>
CREATE TABLE dbo.Users
(
    Id                INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    UserName          NVARCHAR(50)       NOT NULL CONSTRAINT UQ_Users_UserName UNIQUE,
    PasswordHash      NVARCHAR(256)      NOT NULL,
    CreatedAtUtc      DATETIME2(0)       NOT NULL,
    LastSignInAtUtc   DATETIME2(0)       NULL,
    FailedSignInCount INT                NOT NULL CONSTRAINT DF_Users_FailedSignInCount DEFAULT (0),
    LockoutEndUtc     DATETIME2(0)       NULL
);
