-- =============================================================================
-- Demo data for the Warehouse Management System. Run after schema.sql.
--
-- Sign in with:  demo / Demo@12345
--
-- All companies, people and addresses are fictional (.example domains, 555 numbers).
-- Stock figures are consistent with the ledger: QuantityOnHand = SUM(StockMovements)
-- and QuantityReserved = quantities on picked (not yet shipped) orders.
-- =============================================================================

SET NOCOUNT ON;

DECLARE @now DATETIME2(0) = SYSUTCDATETIME();

SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products (Id, Sku, Name, Description, WeightKg, Dimensions, CountryOfOrigin, ReorderLevel, QuantityOnHand, QuantityReserved) VALUES
    (1, N'PKG-1001', N'Stretch Wrap Film 500 mm',       N'Cast stretch film, 23 µm, 300 m roll',            2.500, N'50 × 10 × 10 cm',    'DE',  40, 150, 24),
    (2, N'PKG-1002', N'Shipping Box M',                 N'Double-wall corrugated box, 400 × 300 × 300 mm', 0.450, N'40 × 30 × 30 cm',    'NL', 100, 280,  0),
    (3, N'PKG-1003', N'Shipping Box L',                 N'Double-wall corrugated box, 600 × 400 × 400 mm', 0.800, N'60 × 40 × 40 cm',    'NL',  50,  80,  0),
    (4, N'PKG-1004', N'Packing Tape 48 mm',             N'Low-noise polypropylene tape, 66 m roll',         0.250, N'10 × 10 × 5 cm',     'PL',  60, 160,  0),
    (5, N'SCN-2001', N'Handheld 2D Barcode Scanner',    N'Rugged scanner with USB charging cradle',         0.300, N'18 × 8 × 6 cm',      'CZ',  10,   7,  0),
    (6, N'LBL-3001', N'Thermal Labels 100 × 150 mm',    N'Direct thermal shipping labels, 500 per roll',    1.100, N'12 × 12 × 10 cm',    'DE',  40,  30,  0),
    (7, N'PLT-4001', N'EUR Pallet 1200 × 800 mm',       N'EPAL certified wooden pallet',                   25.000, N'120 × 80 × 14.4 cm', 'PL',  25,  50, 20),
    (8, N'SAF-5001', N'Cut-Resistant Gloves (pair)',    N'Level C cut protection, size L',                  0.080, NULL,                  'PT', 100, 300,  0),
    (9, N'SAF-5002', N'High-Visibility Vest',           N'EN ISO 20471 class 2, size XL',                   0.150, NULL,                  'PT',  20,   0,  0);
SET IDENTITY_INSERT dbo.Products OFF;

SET IDENTITY_INSERT dbo.Customers ON;
INSERT INTO dbo.Customers (Id, Name, ContactName, Email, Phone, Address) VALUES
    (1, N'Rhein Retail GmbH',  N'Jonas Weber',    N'orders@rhein-retail.example',      N'+49 211 555 0100', N'Königsallee 12, 40212 Düsseldorf, DE'),
    (2, N'Maas Supplies B.V.', N'Sanne de Vries', N'purchasing@maas-supplies.example', N'+31 10 555 0142',  N'Coolsingel 40, 3011 AD Rotterdam, NL'),
    (3, N'Nordic Home AB',     N'Erik Lindqvist', N'logistics@nordichome.example',     N'+46 31 555 0177',  N'Avenyn 8, 411 36 Göteborg, SE'),
    (4, N'Alpine Outdoor AG',  N'Lea Brunner',    N'supply@alpine-outdoor.example',    N'+41 44 555 0123',  N'Bahnhofstrasse 55, 8001 Zürich, CH');
SET IDENTITY_INSERT dbo.Customers OFF;

SET IDENTITY_INSERT dbo.Carriers ON;
INSERT INTO dbo.Carriers (Id, Name, ContactName, Email, Phone, Address) VALUES
    (1, N'FastFreight Logistics', N'Dispatch desk', N'dispatch@fastfreight.example', N'+49 203 555 0190', N'Hafenstrasse 5, 47119 Duisburg, DE'),
    (2, N'EuroParcel Express',    N'Customer care', N'support@europarcel.example',   N'+31 20 555 0111',  N'Schiphol Boulevard 101, 1118 BG Schiphol, NL'),
    (3, N'Northline Haulage',     N'Planning team', N'planning@northline.example',   N'+46 8 555 0166',   N'Hamnvägen 3, 115 74 Stockholm, SE');
SET IDENTITY_INSERT dbo.Carriers OFF;

SET IDENTITY_INSERT dbo.Orders ON;
INSERT INTO dbo.Orders (Id, CustomerId, Status, CustomerReference, CreatedAtUtc, CreatedBy, PickedAtUtc, PickedBy, ShippedAtUtc, ShippedBy, CancelledAtUtc, CancelledBy, CarrierId, TrackingNumber) VALUES
    (1, 1, 2, N'RR-PO-5531', DATEADD(DAY, -7, @now),   N'demo', DATEADD(DAY, -6, @now), N'demo', DATEADD(DAY, -5, @now), N'demo', NULL, NULL, 1, N'FF-7731-0042'),
    (2, 2, 2, N'MS-20817',   DATEADD(DAY, -4, @now),   N'demo', DATEADD(DAY, -3, @now), N'demo', DATEADD(DAY, -2, @now), N'demo', NULL, NULL, 2, N'EPX4405129'),
    (3, 3, 1, N'NH-1192',    DATEADD(DAY, -2, @now),   N'demo', DATEADD(DAY, -1, @now), N'demo', NULL, NULL, NULL, NULL, NULL, NULL),
    (4, 4, 0, N'AO-7740',    DATEADD(DAY, -1, @now),   N'demo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (5, 1, 0, N'RR-PO-5602', DATEADD(HOUR, -6, @now),  N'demo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (6, 2, 3, NULL,          DATEADD(DAY, -6, @now),   N'demo', NULL, NULL, NULL, NULL, DATEADD(DAY, -5, @now), N'demo', NULL, NULL),
    (7, 3, 0, N'NH-1207',    DATEADD(HOUR, -3, @now),  N'demo', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT dbo.Orders OFF;

INSERT INTO dbo.OrderLines (OrderId, ProductId, Quantity) VALUES
    (1, 2, 120), (1, 4, 40),   -- SO-00001 shipped
    (2, 5, 5),   (2, 6, 30),   -- SO-00002 shipped
    (3, 1, 24),  (3, 7, 20),   -- SO-00003 picked: stock reserved
    (4, 3, 60),  (4, 8, 100),  -- SO-00004 open, ready to pick
    (5, 5, 20),  (5, 4, 30),   -- SO-00005 open, short on SCN-2001
    (6, 1, 10),                -- SO-00006 cancelled
    (7, 6, 10);                -- SO-00007 open, ready to pick

INSERT INTO dbo.StockMovements (ProductId, Type, Quantity, Reference, OrderId, CreatedAtUtc, CreatedBy) VALUES
    (1, 1,  150, N'PO-24017',  NULL, DATEADD(DAY, -14, @now), N'demo'),
    (2, 1,  400, N'PO-24017',  NULL, DATEADD(DAY, -14, @now), N'demo'),
    (3, 1,   80, N'PO-24021',  NULL, DATEADD(DAY, -12, @now), N'demo'),
    (4, 1,  200, N'PO-24021',  NULL, DATEADD(DAY, -12, @now), N'demo'),
    (5, 1,   12, N'PO-24025',  NULL, DATEADD(DAY, -10, @now), N'demo'),
    (6, 1,   60, N'PO-24025',  NULL, DATEADD(DAY, -10, @now), N'demo'),
    (7, 1,   50, N'DN-88412',  NULL, DATEADD(DAY,  -9, @now), N'demo'),
    (8, 1,  300, N'DN-88415',  NULL, DATEADD(DAY,  -8, @now), N'demo'),
    (2, 2, -120, N'SO-00001',  1,    DATEADD(DAY,  -5, @now), N'demo'),
    (4, 2,  -40, N'SO-00001',  1,    DATEADD(DAY,  -5, @now), N'demo'),
    (5, 2,   -5, N'SO-00002',  2,    DATEADD(DAY,  -2, @now), N'demo'),
    (6, 2,  -30, N'SO-00002',  2,    DATEADD(DAY,  -2, @now), N'demo');

-- Password: Demo@12345 (PBKDF2-HMAC-SHA512, 210,000 iterations)
INSERT INTO dbo.Users (UserName, PasswordHash, CreatedAtUtc) VALUES
    (N'demo', N'PBKDF2-SHA512$210000$FzUv0fXvoDAOABGLVMJEaA==$ep9ZRi3ZFAB9/M5pwoSQRcSw3j8NwqWTJIy3Bp787oc=', DATEADD(DAY, -14, @now));
