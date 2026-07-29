IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(150) NOT NULL,
        [IconName] nvarchar(100) NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] int NOT NULL DEFAULT 0,
        [AvatarUrl] nvarchar(1000) NULL,
        [IsBiometricEnabled] bit NOT NULL DEFAULT CAST(0 AS bit),
        [LastLoginAt] datetime2 NULL,
        [PreferredLanguage] nvarchar(10) NOT NULL DEFAULT N'en',
        [PreferredCurrency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [RowVersion] rowversion NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [SubCategories] (
        [Id] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Slug] nvarchar(150) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SubCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Businesses] (
        [Id] uniqueidentifier NOT NULL,
        [OwnerId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(250) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Email] nvarchar(256) NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [AddressLine1] nvarchar(200) NOT NULL,
        [AddressLine2] nvarchar(200) NULL,
        [City] nvarchar(100) NOT NULL,
        [State] nvarchar(100) NULL,
        [PostalCode] nvarchar(20) NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Latitude] float NULL,
        [Longitude] float NULL,
        [Website] nvarchar(500) NULL,
        [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [BookingType] int NOT NULL DEFAULT 0,
        [CancellationPolicy] nvarchar(2000) NULL,
        [TimeZone] nvarchar(100) NOT NULL DEFAULT N'UTC',
        [Currency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [AverageRating] decimal(2,1) NOT NULL DEFAULT 0.0,
        [TotalReviews] int NOT NULL DEFAULT 0,
        [CoverImageUrl] nvarchar(1000) NULL,
        [LogoUrl] nvarchar(1000) NULL,
        [RowVersion] rowversion NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Businesses] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Businesses_AverageRating] CHECK ([AverageRating] >= 0 AND [AverageRating] <= 5),
        CONSTRAINT [CK_Businesses_TotalReviews] CHECK ([TotalReviews] >= 0),
        CONSTRAINT [FK_Businesses_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL DEFAULT 6,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(2000) NOT NULL,
        [Data] nvarchar(max) NULL,
        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    DECLARE @defaultSchema AS sysname;
    SET @defaultSchema = SCHEMA_NAME();
    DECLARE @description AS sql_variant;
    SET @description = N'JSON payload with notification metadata';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Notifications', 'COLUMN', N'Data';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(500) NOT NULL,
        [JwtId] nvarchar(200) NOT NULL,
        [IsUsed] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsRevoked] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [UserPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Language] nvarchar(10) NOT NULL DEFAULT N'en',
        [Currency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [Interests] nvarchar(max) NULL,
        [IsDarkMode] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsAmoledMode] bit NOT NULL DEFAULT CAST(0 AS bit),
        [NotificationsEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [MarketingEmails] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    DECLARE @defaultSchema1 AS sysname;
    SET @defaultSchema1 = SCHEMA_NAME();
    DECLARE @description1 AS sql_variant;
    SET @description1 = N'JSON array of interest IDs';
    EXEC sp_addextendedproperty 'MS_Description', @description1, 'SCHEMA', @defaultSchema1, 'TABLE', N'UserPreferences', 'COLUMN', N'Interests';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [BusinessCategories] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BusinessCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessCategories_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BusinessCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [BusinessImages] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Url] nvarchar(1000) NOT NULL,
        [AltText] nvarchar(500) NULL,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [IsCover] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BusinessImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BusinessImages_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Providers] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NULL,
        [Bio] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Providers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Providers_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Providers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Services] (
        [Id] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [DurationMinutes] int NOT NULL,
        [PriceAmount] decimal(18,2) NOT NULL,
        [PriceCurrency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [Category] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Services_DurationMinutes] CHECK ([DurationMinutes] >= 5 AND [DurationMinutes] <= 1440),
        CONSTRAINT [CK_Services_PriceAmount] CHECK ([PriceAmount] >= 0),
        CONSTRAINT [FK_Services_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [ProviderAvailabilities] (
        [Id] uniqueidentifier NOT NULL,
        [ProviderId] uniqueidentifier NOT NULL,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [IsAvailable] bit NOT NULL DEFAULT CAST(1 AS bit),
        [SlotDurationMinutes] int NOT NULL DEFAULT 60,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderAvailabilities] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProviderAvailabilities_SlotDuration] CHECK ([SlotDurationMinutes] >= 15 AND [SlotDurationMinutes] <= 480),
        CONSTRAINT [CK_ProviderAvailabilities_TimeRange] CHECK ([EndTime] > [StartTime]),
        CONSTRAINT [FK_ProviderAvailabilities_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [ProviderAvailabilityOverrides] (
        [Id] uniqueidentifier NOT NULL,
        [ProviderId] uniqueidentifier NOT NULL,
        [Date] date NOT NULL,
        [StartTime] time NULL,
        [EndTime] time NULL,
        [IsAvailable] bit NOT NULL,
        [Reason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderAvailabilityOverrides] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderAvailabilityOverrides_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] uniqueidentifier NOT NULL,
        [BookingReference] nvarchar(20) NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [ProviderId] uniqueidentifier NOT NULL,
        [ServiceId] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CustomerNotes] nvarchar(1000) NULL,
        [IsCustomerNotified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [TotalAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT N'USD',
        [CancellationReason] nvarchar(500) NULL,
        [RescheduledFromId] uniqueidentifier NULL,
        [RowVersion] rowversion NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Appointments_TimeRange] CHECK ([EndTime] > [StartTime]),
        CONSTRAINT [CK_Appointments_TotalAmount] CHECK ([TotalAmount] >= 0),
        CONSTRAINT [FK_Appointments_Appointments_RescheduledFromId] FOREIGN KEY ([RescheduledFromId]) REFERENCES [Appointments] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Appointments_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [ProviderServices] (
        [Id] uniqueidentifier NOT NULL,
        [ProviderId] uniqueidentifier NOT NULL,
        [ServiceId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProviderServices_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProviderServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [AppointmentLogs] (
        [Id] uniqueidentifier NOT NULL,
        [AppointmentId] uniqueidentifier NOT NULL,
        [FromStatus] int NULL,
        [ToStatus] int NOT NULL,
        [ChangedByUserId] uniqueidentifier NULL,
        [Reason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AppointmentLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AppointmentLogs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] uniqueidentifier NOT NULL,
        [AppointmentId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [PaymentMethod] int NOT NULL DEFAULT 0,
        [Status] int NOT NULL DEFAULT 0,
        [TransactionId] nvarchar(200) NULL,
        [IsDeposit] bit NOT NULL DEFAULT CAST(0 AS bit),
        [RefundAmount] decimal(18,2) NULL,
        [RefundReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Payments_Amount] CHECK ([Amount] >= 0),
        CONSTRAINT [CK_Payments_RefundAmount] CHECK ([RefundAmount] IS NULL OR [RefundAmount] >= 0),
        CONSTRAINT [FK_Payments_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payments_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] uniqueidentifier NOT NULL,
        [AppointmentId] uniqueidentifier NOT NULL,
        [BusinessId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(2000) NULL,
        [IsVerifiedPurchase] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsPublished] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Reviews_Rating] CHECK ([Rating] >= 1 AND [Rating] <= 5),
        CONSTRAINT [FK_Reviews_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reviews_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE TABLE [PaymentTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [PaymentId] uniqueidentifier NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ProviderResponse] nvarchar(max) NULL,
        [IsSuccess] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PaymentTransactions_Amount] CHECK ([Amount] >= 0),
        CONSTRAINT [FK_PaymentTransactions_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_AppointmentLogs_AppointmentId] ON [AppointmentLogs] ([AppointmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Appointments_BookingReference] ON [Appointments] ([BookingReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_BusinessId_StartTime] ON [Appointments] ([BusinessId], [StartTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_ProviderId] ON [Appointments] ([ProviderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_RescheduledFromId] ON [Appointments] ([RescheduledFromId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Appointments_StartTime] ON [Appointments] ([StartTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessCategories_BusinessId_CategoryId] ON [BusinessCategories] ([BusinessId], [CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_BusinessCategories_CategoryId] ON [BusinessCategories] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Businesses_AverageRating] ON [Businesses] ([AverageRating] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Businesses_City_Country] ON [Businesses] ([City], [Country]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Businesses_OwnerId] ON [Businesses] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Businesses_Slug] ON [Businesses] ([Slug]) WHERE IsDeleted = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_BusinessImages_BusinessId_DisplayOrder] ON [BusinessImages] ([BusinessId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_CreatedAt] ON [Notifications] ([UserId], [CreatedAt] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_AppointmentId] ON [Payments] ([AppointmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Payments_CustomerId] ON [Payments] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_PaymentId] ON [PaymentTransactions] ([PaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderAvailabilities_ProviderId_DayOfWeek] ON [ProviderAvailabilities] ([ProviderId], [DayOfWeek]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AvailabilityOverrides_ProviderId_Date] ON [ProviderAvailabilityOverrides] ([ProviderId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Providers_BusinessId] ON [Providers] ([BusinessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Providers_UserId] ON [Providers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProviderServices_ProviderId_ServiceId] ON [ProviderServices] ([ProviderId], [ServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_ProviderServices_ServiceId] ON [ProviderServices] ([ServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reviews_AppointmentId] ON [Reviews] ([AppointmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Reviews_BusinessId] ON [Reviews] ([BusinessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Reviews_CustomerId] ON [Reviews] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Services_BusinessId] ON [Services] ([BusinessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Services_BusinessId_DisplayOrder] ON [Services] ([BusinessId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_SubCategories_CategoryId] ON [SubCategories] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Users_CreatedAt] ON [Users] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE IsDeleted = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    CREATE INDEX [IX_Users_Role] ON [Users] ([Role]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728214417_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728214417_Initial', N'10.0.10');
END;

COMMIT;
GO

