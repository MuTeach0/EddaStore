BEGIN TRANSACTION;
ALTER TABLE [Orders] ADD [CheckoutToken] nvarchar(64) NOT NULL DEFAULT N'';

ALTER TABLE [Orders] ADD [CouponCode] nvarchar(50) NULL;

CREATE TABLE [WishlistItems] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [AddedAt] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_WishlistItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WishlistItems_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_WishlistItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

UPDATE Orders SET CheckoutToken = LOWER(CONVERT(nvarchar(36), NEWID())) WHERE CheckoutToken IS NULL OR CheckoutToken = '';

CREATE UNIQUE INDEX [IX_Orders_CustomerId_CheckoutToken] ON [Orders] ([CustomerId], [CheckoutToken]);

CREATE UNIQUE INDEX [IX_WishlistItems_CustomerId_ProductId] ON [WishlistItems] ([CustomerId], [ProductId]) WHERE [IsDeleted] = 0;

CREATE INDEX [IX_WishlistItems_ProductId] ON [WishlistItems] ([ProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261023073906_AddCheckoutIdempotencyAndWishlist', N'10.0.13');

COMMIT;
GO

