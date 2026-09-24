CREATE TABLE [dbo].[Inventory]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ProductId] INT NOT NULL, 
    [Quntity] INT NOT NULL, 
    [PurchaesPrice] MONEY NOT NULL, 
    [PurchaesDate] DATETIME2 NOT NULL DEFAULT getutcdate()
)
