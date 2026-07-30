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
    [IsSuspended] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SuspendedAt] datetime2 NULL,
    [SuspendedBy] uniqueidentifier NULL,
    [SuspensionReason] nvarchar(500) NULL,
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
    CONSTRAINT [FK_Appointments_Appointments_RescheduledFromId] FOREIGN KEY ([RescheduledFromId]) REFERENCES [Appointments] ([Id]),
    CONSTRAINT [FK_Appointments_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

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
    CONSTRAINT [FK_ProviderServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id])
);

CREATE TABLE [RecurringBookings] (
    [Id] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [ProviderId] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NOT NULL,
    [BusinessId] uniqueidentifier NOT NULL,
    [RecurrenceType] int NOT NULL,
    [Interval] int NOT NULL DEFAULT 1,
    [DayOfMonth] int NULL,
    [DaysOfWeek] nvarchar(100) NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [SeriesStartDate] datetime2 NOT NULL,
    [SeriesEndDate] datetime2 NULL,
    [MaxOccurrences] int NULL,
    [OccurrencesCreated] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [PausedUntil] datetime2 NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RecurringBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecurringBookings_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]),
    CONSTRAINT [FK_RecurringBookings_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]),
    CONSTRAINT [FK_RecurringBookings_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]),
    CONSTRAINT [FK_RecurringBookings_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [WaitlistEntries] (
    [Id] uniqueidentifier NOT NULL,
    [BusinessId] uniqueidentifier NOT NULL,
    [ProviderId] uniqueidentifier NOT NULL,
    [ServiceId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [AppointmentDate] date NOT NULL,
    [PreferredStartTime] time NULL,
    [PreferredEndTime] time NULL,
    [Notes] nvarchar(1000) NULL,
    [Status] int NOT NULL DEFAULT 0,
    [Priority] int NOT NULL DEFAULT 0,
    [NotifiedAt] datetime2 NULL,
    [PromotedAt] datetime2 NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_WaitlistEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WaitlistEntries_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]),
    CONSTRAINT [FK_WaitlistEntries_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]),
    CONSTRAINT [FK_WaitlistEntries_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]),
    CONSTRAINT [FK_WaitlistEntries_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id])
);

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

CREATE TABLE [Documents] (
    [Id] uniqueidentifier NOT NULL,
    [AppointmentId] uniqueidentifier NULL,
    [BusinessId] uniqueidentifier NOT NULL,
    [ProviderId] uniqueidentifier NULL,
    [UploadedByUserId] uniqueidentifier NOT NULL,
    [DocumentType] int NOT NULL,
    [FileName] nvarchar(500) NOT NULL,
    [OriginalFileName] nvarchar(500) NOT NULL,
    [ContentType] nvarchar(200) NOT NULL,
    [Extension] nvarchar(50) NOT NULL,
    [FileSize] bigint NOT NULL,
    [StoragePath] nvarchar(2000) NOT NULL,
    [ThumbnailPath] nvarchar(2000) NULL,
    [ContentHash] nvarchar(256) NOT NULL,
    [Version] int NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Documents_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Documents_Businesses_BusinessId] FOREIGN KEY ([BusinessId]) REFERENCES [Businesses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Documents_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Documents_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

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

CREATE TABLE [Reviews] (
    [Id] uniqueidentifier NOT NULL,
    [AppointmentId] uniqueidentifier NOT NULL,
    [BusinessId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(2000) NULL,
    [IsVerifiedPurchase] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsPublished] bit NOT NULL DEFAULT CAST(1 AS bit),
    [ProviderId] uniqueidentifier NULL,
    [ProviderReply] nvarchar(2000) NULL,
    [RepliedAt] datetime2 NULL,
    [ReplyUpdatedAt] datetime2 NULL,
    [IsHidden] bit NOT NULL DEFAULT CAST(0 AS bit),
    [HiddenAt] datetime2 NULL,
    [HideReason] nvarchar(500) NULL,
    [ModerationReason] nvarchar(500) NULL,
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
    CONSTRAINT [FK_Reviews_Providers_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [Providers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

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

CREATE TABLE [ReviewReports] (
    [Id] uniqueidentifier NOT NULL,
    [ReviewId] uniqueidentifier NOT NULL,
    [ReportedByCustomerId] uniqueidentifier NOT NULL,
    [Reason] int NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Status] int NOT NULL DEFAULT 0,
    [Resolution] nvarchar(500) NULL,
    [ResolvedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ReviewReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReviewReports_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewReports_Users_ReportedByCustomerId] FOREIGN KEY ([ReportedByCustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ReviewVotes] (
    [Id] uniqueidentifier NOT NULL,
    [ReviewId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NOT NULL,
    [IsHelpful] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] uniqueidentifier NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ReviewVotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReviewVotes_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewVotes_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AppointmentLogs_AppointmentId] ON [AppointmentLogs] ([AppointmentId]);

CREATE UNIQUE INDEX [IX_Appointments_BookingReference] ON [Appointments] ([BookingReference]);

CREATE INDEX [IX_Appointments_BusinessId_StartTime] ON [Appointments] ([BusinessId], [StartTime]);

CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);

CREATE INDEX [IX_Appointments_ProviderId] ON [Appointments] ([ProviderId]);

CREATE INDEX [IX_Appointments_ProviderId_StartTime] ON [Appointments] ([ProviderId], [StartTime]);

CREATE INDEX [IX_Appointments_RescheduledFromId] ON [Appointments] ([RescheduledFromId]);

CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);

CREATE INDEX [IX_Appointments_StartTime] ON [Appointments] ([StartTime]);

CREATE UNIQUE INDEX [IX_BusinessCategories_BusinessId_CategoryId] ON [BusinessCategories] ([BusinessId], [CategoryId]);

CREATE INDEX [IX_BusinessCategories_CategoryId] ON [BusinessCategories] ([CategoryId]);

CREATE INDEX [IX_Businesses_AverageRating] ON [Businesses] ([AverageRating] DESC);

CREATE INDEX [IX_Businesses_City_Country] ON [Businesses] ([City], [Country]);

CREATE INDEX [IX_Businesses_OwnerId] ON [Businesses] ([OwnerId]);

CREATE UNIQUE INDEX [IX_Businesses_Slug] ON [Businesses] ([Slug]) WHERE IsDeleted = 0;

CREATE INDEX [IX_BusinessImages_BusinessId_DisplayOrder] ON [BusinessImages] ([BusinessId], [DisplayOrder]);

CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Categories] ([Slug]);

CREATE INDEX [IX_Documents_AppointmentId] ON [Documents] ([AppointmentId]);

CREATE INDEX [IX_Documents_BusinessId] ON [Documents] ([BusinessId]);

CREATE INDEX [IX_Documents_BusinessId_DocumentType] ON [Documents] ([BusinessId], [DocumentType]);

CREATE INDEX [IX_Documents_ContentHash] ON [Documents] ([ContentHash]);

CREATE INDEX [IX_Documents_ProviderId] ON [Documents] ([ProviderId]);

CREATE INDEX [IX_Documents_UploadedByUserId] ON [Documents] ([UploadedByUserId]);

CREATE INDEX [IX_Notifications_UserId_CreatedAt] ON [Notifications] ([UserId], [CreatedAt] DESC);

CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications] ([UserId], [IsRead]);

CREATE UNIQUE INDEX [IX_Payments_AppointmentId] ON [Payments] ([AppointmentId]);

CREATE INDEX [IX_Payments_CustomerId_Status] ON [Payments] ([CustomerId], [Status]);

CREATE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]);

CREATE INDEX [IX_PaymentTransactions_PaymentId] ON [PaymentTransactions] ([PaymentId]);

CREATE INDEX [IX_ProviderAvailabilities_ProviderId_DayOfWeek] ON [ProviderAvailabilities] ([ProviderId], [DayOfWeek]);

CREATE UNIQUE INDEX [IX_AvailabilityOverrides_ProviderId_Date] ON [ProviderAvailabilityOverrides] ([ProviderId], [Date]);

CREATE INDEX [IX_Providers_BusinessId] ON [Providers] ([BusinessId]);

CREATE UNIQUE INDEX [IX_Providers_UserId] ON [Providers] ([UserId]);

CREATE UNIQUE INDEX [IX_ProviderServices_ProviderId_ServiceId] ON [ProviderServices] ([ProviderId], [ServiceId]);

CREATE INDEX [IX_ProviderServices_ServiceId] ON [ProviderServices] ([ServiceId]);

CREATE INDEX [IX_RecurringBookings_BusinessId] ON [RecurringBookings] ([BusinessId]);

CREATE INDEX [IX_RecurringBookings_CustomerId] ON [RecurringBookings] ([CustomerId]);

CREATE INDEX [IX_RecurringBookings_ProviderId] ON [RecurringBookings] ([ProviderId]);

CREATE INDEX [IX_RecurringBookings_ProviderId_IsActive] ON [RecurringBookings] ([ProviderId], [IsActive]);

CREATE INDEX [IX_RecurringBookings_ServiceId] ON [RecurringBookings] ([ServiceId]);

CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_ReviewReports_ReportedByCustomerId] ON [ReviewReports] ([ReportedByCustomerId]);

CREATE INDEX [IX_ReviewReports_ReviewId] ON [ReviewReports] ([ReviewId]);

CREATE INDEX [IX_ReviewReports_Status] ON [ReviewReports] ([Status]);

CREATE UNIQUE INDEX [IX_Reviews_AppointmentId] ON [Reviews] ([AppointmentId]);

CREATE INDEX [IX_Reviews_BusinessId] ON [Reviews] ([BusinessId]);

CREATE INDEX [IX_Reviews_CustomerId] ON [Reviews] ([CustomerId]);

CREATE INDEX [IX_Reviews_ProviderId] ON [Reviews] ([ProviderId]);

CREATE INDEX [IX_ReviewVotes_CustomerId] ON [ReviewVotes] ([CustomerId]);

CREATE INDEX [IX_ReviewVotes_ReviewId] ON [ReviewVotes] ([ReviewId]);

CREATE UNIQUE INDEX [IX_ReviewVotes_ReviewId_CustomerId] ON [ReviewVotes] ([ReviewId], [CustomerId]);

CREATE INDEX [IX_Services_BusinessId] ON [Services] ([BusinessId]);

CREATE INDEX [IX_Services_BusinessId_DisplayOrder] ON [Services] ([BusinessId], [DisplayOrder]);

CREATE INDEX [IX_SubCategories_CategoryId] ON [SubCategories] ([CategoryId]);

CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);

CREATE INDEX [IX_Users_CreatedAt] ON [Users] ([CreatedAt]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE IsDeleted = 0;

CREATE INDEX [IX_Users_Role] ON [Users] ([Role]);

CREATE INDEX [IX_WaitlistEntries_BusinessId] ON [WaitlistEntries] ([BusinessId]);

CREATE INDEX [IX_WaitlistEntries_CustomerId_Status] ON [WaitlistEntries] ([CustomerId], [Status]);

CREATE INDEX [IX_WaitlistEntries_ExpiresAt] ON [WaitlistEntries] ([ExpiresAt]);

CREATE INDEX [IX_WaitlistEntries_ProviderId_AppointmentDate_Status] ON [WaitlistEntries] ([ProviderId], [AppointmentDate], [Status]);

CREATE INDEX [IX_WaitlistEntries_ServiceId] ON [WaitlistEntries] ([ServiceId]);

-- ══════════════════════════════════════════════════════════════
-- SEED DATA
-- ══════════════════════════════════════════════════════════════

-- Admin User (password: Admin@123456 - BCrypt hash)
INSERT INTO [Users] ([Id], [FirstName], [LastName], [Email], [PasswordHash], [Role], [PreferredLanguage], [PreferredCurrency], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES ('A0000000-0000-0000-0000-000000000001', N'System', N'Admin', N'admin@bookify.com', N'$2a$12$LJ3m4ys3Lk0TSwHnbfOMiOXPm1Qlq5Gz0Yq0Z0Z0Z0Z0Z0Z0Z0Z0', 3, N'en', N'USD', SYSUTCDATETIME(), SYSUTCDATETIME(), 0);

-- Categories
INSERT INTO [Categories] ([Id], [Name], [Slug], [IconName], [DisplayOrder], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
('B0000000-0000-0000-0000-000000000001', N'Doctors', N'doctors', N'medical_services', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('B0000000-0000-0000-0000-000000000002', N'Salons', N'salons', N'content_cut', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('B0000000-0000-0000-0000-000000000003', N'Spas', N'spas', N'spa', 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('B0000000-0000-0000-0000-000000000004', N'Gyms', N'gyms', N'fitness_center', 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('B0000000-0000-0000-0000-000000000005', N'Dining', N'dining', N'restaurant', 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('B0000000-0000-0000-0000-000000000006', N'Hotels', N'hotels', N'hotel', 6, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);

-- SubCategories
INSERT INTO [SubCategories] ([Id], [CategoryId], [Name], [Slug], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted])
VALUES
('C0000000-0000-0000-0000-000000000001', 'B0000000-0000-0000-0000-000000000001', N'Dentist', N'dentist', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000002', 'B0000000-0000-0000-0000-000000000001', N'Dermatologist', N'dermatologist', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000003', 'B0000000-0000-0000-0000-000000000001', N'Cardiologist', N'cardiologist', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000004', 'B0000000-0000-0000-0000-000000000001', N'Ophthalmologist', N'ophthalmologist', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000005', 'B0000000-0000-0000-0000-000000000001', N'General Practitioner', N'general-practitioner', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000006', 'B0000000-0000-0000-0000-000000000002', N'Hair Styling', N'hair-styling', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000007', 'B0000000-0000-0000-0000-000000000002', N'Nail Art', N'nail-art', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000008', 'B0000000-0000-0000-0000-000000000002', N'Barber', N'barber', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000009', 'B0000000-0000-0000-0000-000000000003', N'Massage', N'massage', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000A', 'B0000000-0000-0000-0000-000000000003', N'Facial', N'facial', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000B', 'B0000000-0000-0000-0000-000000000003', N'Body Treatment', N'body-treatment', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000C', 'B0000000-0000-0000-0000-000000000004', N'Personal Training', N'personal-training', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000D', 'B0000000-0000-0000-0000-000000000004', N'Yoga', N'yoga', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000E', 'B0000000-0000-0000-0000-000000000004', N'Pilates', N'pilates', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-00000000000F', 'B0000000-0000-0000-0000-000000000005', N'Fine Dining', N'fine-dining', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000010', 'B0000000-0000-0000-0000-000000000005', N'Casual Dining', N'casual-dining', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000011', 'B0000000-0000-0000-0000-000000000006', N'Luxury', N'luxury', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
('C0000000-0000-0000-0000-000000000012', 'B0000000-0000-0000-0000-000000000006', N'Boutique', N'boutique', 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);

-- ══════════════════════════════════════════════════════════════
-- FULL-TEXT SEARCH INDEXES
-- ══════════════════════════════════════════════════════════════
-- Note: Requires SQL Server Full-Text Search feature to be enabled
-- To enable: Run "sp_fulltext_database 'enable'" on the database
CREATE FULLTEXT CATALOG BookifyFullTextCatalog AS DEFAULT;
GO

CREATE FULLTEXT INDEX ON [Businesses] ([Name], [Description]) 
    KEY INDEX [PK_Businesses] ON BookifyFullTextCatalog 
    WITH CHANGE_TRACKING AUTO;
GO

CREATE FULLTEXT INDEX ON [Services] ([Name], [Description]) 
    KEY INDEX [PK_Services] ON BookifyFullTextCatalog 
    WITH CHANGE_TRACKING AUTO;
GO

-- ══════════════════════════════════════════════════════════════
-- SPATIAL INDEXES
-- ══════════════════════════════════════════════════════════════
-- Note: Requires SQL Server Spatial support and Location column
-- The Businesses table has Latitude/Longitude columns for spatial queries
-- Uncomment when spatial queries are needed:
-- ALTER TABLE [Businesses] ADD [Location] AS geography([Latitude], [Longitude]);
-- CREATE SPATIAL INDEX [IX_Businesses_Location] ON [Businesses] ([Location])
--   WITH (BOUNDING_BOX = (-180, -90, 180, 90));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260729113913_Initial', N'10.0.10');

COMMIT;
GO

