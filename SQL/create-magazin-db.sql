-- Lucrarea 5, Sarcina 2: baza de date "Magazin" (cosul de cumparaturi al unui magazin virtual).
-- Aceeasi schema ca in Lucrarea 4 (Products, Customers, [Order], OrderItem), la care se adauga coloana Products.Stock,
-- necesara pentru verificarea stocului din baza de date. Script idempotent: poate fi rulat de mai multe ori fara eroare
-- si poate fi rulat si peste baza creata in Lucrarea 4 (adauga doar coloana lipsa).
-- Nota: ORDER este cuvant rezervat in SQL, deci tabela [Order] se scrie intotdeauna cu paranteze drepte.

IF DB_ID(N'Magazin') IS NULL
BEGIN
    CREATE DATABASE [Magazin];
END
GO

USE [Magazin];
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Products]
    (
        [Id]           INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED,
        [Code]         VARCHAR(20)    NOT NULL CONSTRAINT [UQ_Products_Code] UNIQUE,
        [Name]         NVARCHAR(100)  NOT NULL,
        [Price]        DECIMAL(10, 2) NOT NULL CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0),
        -- Unitatea de masura in care se comanda produsul (buc, kg, l, m ...).
        [QuantityType] NVARCHAR(10)   NOT NULL,
        -- Cantitatea disponibila in stoc; scade la fiecare comanda plasata.
        [Stock]        INT            NOT NULL CONSTRAINT [DF_Products_Stock] DEFAULT (0) CONSTRAINT [CK_Products_Stock] CHECK ([Stock] >= 0)
    );
END
GO

-- Pentru baza creata deja in Lucrarea 4: adauga coloana Stock si un stoc de pornire pentru produsele de test.
IF COL_LENGTH(N'dbo.Products', N'Stock') IS NULL
BEGIN
    ALTER TABLE [dbo].[Products]
        ADD [Stock] INT NOT NULL CONSTRAINT [DF_Products_Stock] DEFAULT (0) CONSTRAINT [CK_Products_Stock] CHECK ([Stock] >= 0);

    -- EXEC: coloana Stock nu exista cand se compileaza acest lot, deci UPDATE-ul trebuie compilat dupa ALTER.
    EXEC (N'UPDATE [dbo].[Products]
            SET [Stock] = CASE [Code] WHEN ''PR0001'' THEN 10 WHEN ''PR0002'' THEN 5 WHEN ''PR0003'' THEN 2 ELSE 0 END
            WHERE [Code] IN (''PR0001'', ''PR0002'', ''PR0003'', ''PR0004'')');
END
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Customers]
    (
        [Id]   INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED,
        [Code] VARCHAR(20)   NOT NULL CONSTRAINT [UQ_Customers_Code] UNIQUE,
        [Name] NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Order', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Order]
    (
        [Id]              INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED,
        [Date]            DATETIME2 NOT NULL CONSTRAINT [DF_Order_Date] DEFAULT (SYSUTCDATETIME()),
        [DeliveryAddress] NVARCHAR(200) NOT NULL,
        [CustomerId]      INT NOT NULL CONSTRAINT [FK_Order_Customers] REFERENCES [dbo].[Customers]([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.OrderItem', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItem]
    (
        [Id]        INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED,
        [OrderId]   INT NOT NULL CONSTRAINT [FK_OrderItem_Order] REFERENCES [dbo].[Order]([Id]),
        [ProductId] INT NOT NULL CONSTRAINT [FK_OrderItem_Products] REFERENCES [dbo].[Products]([Id]),
        [Quantity]  INT NOT NULL CONSTRAINT [CK_OrderItem_Quantity] CHECK ([Quantity] > 0)
    );
END
GO

-- Date de start (optionale). Numele sunt fara diacritice ca sa nu depinda de codarea cu care sqlcmd citeste fisierul.
-- PR0004 are stoc 0, ca sa se poata proba ramura "stoc insuficient".
MERGE [dbo].[Products] AS target
USING (VALUES
    ('PR0001', N'Mouse optic',        49.90, N'buc', 10),
    ('PR0002', N'Tastatura mecanica', 249.99, N'buc', 5),
    ('PR0003', N'Cablu HDMI',         19.50, N'buc', 2),
    ('PR0004', N'Cafea boabe',        62.00, N'kg',  0)
) AS source ([Code], [Name], [Price], [QuantityType], [Stock])
ON target.[Code] = source.[Code]
WHEN NOT MATCHED THEN
    INSERT ([Code], [Name], [Price], [QuantityType], [Stock]) VALUES (source.[Code], source.[Name], source.[Price], source.[QuantityType], source.[Stock]);
GO

MERGE [dbo].[Customers] AS target
USING (VALUES
    ('CL0001', N'Ion Popescu'),
    ('CL0002', N'Maria Ionescu')
) AS source ([Code], [Name])
ON target.[Code] = source.[Code]
WHEN NOT MATCHED THEN
    INSERT ([Code], [Name]) VALUES (source.[Code], source.[Name]);
GO
