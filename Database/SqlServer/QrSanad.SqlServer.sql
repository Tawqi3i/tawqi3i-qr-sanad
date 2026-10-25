-- sqlcmd defaults QUOTED_IDENTIFIER to OFF, which the filtered unique indexes reject.
SET QUOTED_IDENTIFIER ON;
GO

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
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(128) NOT NULL,
        [Description] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [SecurityAuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(64) NOT NULL,
        [UserId] uniqueidentifier NULL,
        [UserName] nvarchar(256) NULL,
        [ActorId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [IpAddress] nvarchar(64) NULL,
        [UserAgent] nvarchar(512) NULL,
        [Details] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SecurityAuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [NormalizedName] nvarchar(128) NOT NULL,
        [Code] nvarchar(32) NULL,
        [Address] nvarchar(256) NULL,
        [PhoneNumber] nvarchar(32) NULL,
        [Status] nvarchar(32) NOT NULL,
        [AdminUserId] uniqueidentifier NULL,
        [ConcurrencyStamp] nvarchar(64) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(64) NOT NULL,
        [NormalizedUserName] nvarchar(64) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [NormalizedEmail] nvarchar(256) NOT NULL,
        [DisplayName] nvarchar(128) NOT NULL,
        [PhoneNumber] nvarchar(32) NULL,
        [UserType] nvarchar(32) NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [SecurityStamp] nvarchar(64) NOT NULL,
        [ConcurrencyStamp] nvarchar(64) NOT NULL,
        [Status] nvarchar(32) NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [LockoutEnd] datetime2 NULL,
        [LastLoginAt] datetime2 NULL,
        [PasswordChangedAt] datetime2 NULL,
        [MustChangePassword] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [SecurityStamp] nvarchar(64) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedByIp] nvarchar(64) NULL,
        [UserAgent] nvarchar(512) NULL,
        [RevokedAt] datetime2 NULL,
        [RevokedByIp] nvarchar(64) NULL,
        [RevokeReason] nvarchar(64) NULL,
        [ReplacedByTokenId] uniqueidentifier NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE TABLE [UserPermissions] (
        [UserId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        [AssignedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([UserId], [PermissionId]),
        CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPermissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([Id], [Code], [Description])
    VALUES (''6b5dd7de-fed2-8a5b-8064-b4d9a2551f9d'', N''users.update'', N''Update users, change their status and unlock them''),
    (''935d225d-1b6c-830d-916b-c94acbf35197'', N''users.reset_password'', N''Reset another user''''s password''),
    (''9b5270e0-5623-8d12-bf45-ae8cd430b855'', N''users.assign_permissions'', N''Grant and revoke user permissions''),
    (''a1885bbe-d02b-8363-bf99-25e9c4bad98d'', N''users.create'', N''Create users in the branch''),
    (''b5b3840b-f1b5-8912-909e-80df84a42698'', N''users.delete'', N''Delete users''),
    (''c7994376-f8e1-8e70-a2f6-5de3a6ea4963'', N''permissions.view'', N''View the permission catalogue''),
    (''f969d5a8-1f29-8efb-b321-4e7cc4654082'', N''audit_logs.view'', N''View the security audit log of the branch''),
    (''faebb80f-3f91-88d3-8344-93229a4b7798'', N''users.view'', N''View the users of the branch'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Branches_AdminUserId] ON [Branches] ([AdminUserId]) WHERE [AdminUserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]) WHERE [Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_NormalizedName] ON [Branches] ([NormalizedName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Code] ON [Permissions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId_RevokedAt] ON [RefreshTokens] ([UserId], [RevokedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditLogs_BranchId_CreatedAt] ON [SecurityAuditLogs] ([BranchId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditLogs_CreatedAt] ON [SecurityAuditLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditLogs_UserId_CreatedAt] ON [SecurityAuditLogs] ([UserId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_BranchId_UserType] ON [Users] ([BranchId], [UserType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_CreatedAt] ON [Users] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_NormalizedUserName] ON [Users] ([NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [FK_Branches_Users_AdminUserId] FOREIGN KEY ([AdminUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022095756_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022095756_InitialCreate', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022130334_AddESealPermission'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([Id], [Code], [Description])
    VALUES (''8ac78524-d7b8-861f-9978-2a67584e06a8'', N''eseal.view'', N''Retrieve e-sealed documents by SANAD QR code'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Description') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022130334_AddESealPermission'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022130334_AddESealPermission', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024082221_AddPasswordPolicy'
)
BEGIN
    CREATE TABLE [PasswordPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [MinLength] int NOT NULL,
        [MaxLength] int NOT NULL,
        [RequireUppercase] bit NOT NULL,
        [RequireLowercase] bit NOT NULL,
        [RequireDigit] bit NOT NULL,
        [RequireNonAlphanumeric] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_PasswordPolicies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024082221_AddPasswordPolicy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024082221_AddPasswordPolicy', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024123811_AddLicense'
)
BEGIN
    CREATE TABLE [Licenses] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Licenses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261024123811_AddLicense'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261024123811_AddLicense', N'10.0.13');
END;

COMMIT;
GO

