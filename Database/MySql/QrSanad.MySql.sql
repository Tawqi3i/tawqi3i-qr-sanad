CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `migration_id` varchar(150) NOT NULL,
    `product_version` varchar(32) NOT NULL,
    PRIMARY KEY (`migration_id`)
);

START TRANSACTION;
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `permissions` (
        `id` char(36) NOT NULL,
        `code` varchar(128) NOT NULL,
        `description` varchar(256) NOT NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `security_audit_logs` (
        `id` char(36) NOT NULL,
        `event_type` varchar(64) NOT NULL,
        `user_id` char(36) NULL,
        `user_name` varchar(256) NULL,
        `actor_id` char(36) NULL,
        `branch_id` char(36) NULL,
        `ip_address` varchar(64) NULL,
        `user_agent` varchar(512) NULL,
        `details` varchar(2000) NULL,
        `created_at` datetime(6) NOT NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `branches` (
        `id` char(36) NOT NULL,
        `name` varchar(128) NOT NULL,
        `normalized_name` varchar(128) NOT NULL,
        `code` varchar(32) NULL,
        `address` varchar(256) NULL,
        `phone_number` varchar(32) NULL,
        `status` varchar(32) NOT NULL,
        `admin_user_id` char(36) NULL,
        `concurrency_stamp` varchar(64) NOT NULL,
        `is_deleted` tinyint(1) NOT NULL,
        `deleted_at` datetime(6) NULL,
        `deleted_by` char(36) NULL,
        `created_at` datetime(6) NOT NULL,
        `created_by` char(36) NULL,
        `updated_at` datetime(6) NULL,
        `updated_by` char(36) NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `users` (
        `id` char(36) NOT NULL,
        `user_name` varchar(64) NOT NULL,
        `normalized_user_name` varchar(64) NOT NULL,
        `email` varchar(256) NOT NULL,
        `normalized_email` varchar(256) NOT NULL,
        `display_name` varchar(128) NOT NULL,
        `phone_number` varchar(32) NULL,
        `user_type` varchar(32) NOT NULL,
        `branch_id` char(36) NULL,
        `password_hash` varchar(512) NOT NULL,
        `security_stamp` varchar(64) NOT NULL,
        `concurrency_stamp` varchar(64) NOT NULL,
        `status` varchar(32) NOT NULL,
        `access_failed_count` int NOT NULL,
        `lockout_end` datetime(6) NULL,
        `last_login_at` datetime(6) NULL,
        `password_changed_at` datetime(6) NULL,
        `must_change_password` tinyint(1) NOT NULL,
        `is_deleted` tinyint(1) NOT NULL,
        `deleted_at` datetime(6) NULL,
        `deleted_by` char(36) NULL,
        `created_at` datetime(6) NOT NULL,
        `created_by` char(36) NULL,
        `updated_at` datetime(6) NULL,
        `updated_by` char(36) NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `fk_users_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `branches` (`id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `refresh_tokens` (
        `id` char(36) NOT NULL,
        `user_id` char(36) NOT NULL,
        `token_hash` varchar(128) NOT NULL,
        `security_stamp` varchar(64) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `expires_at` datetime(6) NOT NULL,
        `created_by_ip` varchar(64) NULL,
        `user_agent` varchar(512) NULL,
        `revoked_at` datetime(6) NULL,
        `revoked_by_ip` varchar(64) NULL,
        `revoke_reason` varchar(64) NULL,
        `replaced_by_token_id` char(36) NULL,
        PRIMARY KEY (`id`),
        CONSTRAINT `fk_refresh_tokens_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE TABLE `user_permissions` (
        `user_id` char(36) NOT NULL,
        `permission_id` char(36) NOT NULL,
        `assigned_at` datetime(6) NOT NULL,
        `assigned_by` char(36) NULL,
        PRIMARY KEY (`user_id`, `permission_id`),
        CONSTRAINT `fk_user_permissions_permissions_permission_id` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_user_permissions_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('6b5dd7de-fed2-8a5b-8064-b4d9a2551f9d', 'users.update', 'Update users, change their status and unlock them');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('935d225d-1b6c-830d-916b-c94acbf35197', 'users.reset_password', 'Reset another user''s password');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('9b5270e0-5623-8d12-bf45-ae8cd430b855', 'users.assign_permissions', 'Grant and revoke user permissions');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('a1885bbe-d02b-8363-bf99-25e9c4bad98d', 'users.create', 'Create users in the branch');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('b5b3840b-f1b5-8912-909e-80df84a42698', 'users.delete', 'Delete users');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('c7994376-f8e1-8e70-a2f6-5de3a6ea4963', 'permissions.view', 'View the permission catalogue');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('f969d5a8-1f29-8efb-b321-4e7cc4654082', 'audit_logs.view', 'View the security audit log of the branch');
    SELECT ROW_COUNT();

    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('faebb80f-3f91-88d3-8344-93229a4b7798', 'users.view', 'View the users of the branch');
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_branches_admin_user_id` ON `branches` (`admin_user_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_branches_code` ON `branches` (`code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_branches_normalized_name` ON `branches` (`normalized_name`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_permissions_code` ON `permissions` (`code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_refresh_tokens_token_hash` ON `refresh_tokens` (`token_hash`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_refresh_tokens_user_id_revoked_at` ON `refresh_tokens` (`user_id`, `revoked_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_security_audit_logs_branch_id_created_at` ON `security_audit_logs` (`branch_id`, `created_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_security_audit_logs_created_at` ON `security_audit_logs` (`created_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_security_audit_logs_user_id_created_at` ON `security_audit_logs` (`user_id`, `created_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_user_permissions_permission_id` ON `user_permissions` (`permission_id`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_users_branch_id_user_type` ON `users` (`branch_id`, `user_type`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE INDEX `ix_users_created_at` ON `users` (`created_at`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_users_normalized_email` ON `users` (`normalized_email`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX `ix_users_normalized_user_name` ON `users` (`normalized_user_name`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    ALTER TABLE `branches` ADD CONSTRAINT `fk_branches_users_admin_user_id` FOREIGN KEY (`admin_user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022095819_InitialCreate')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
    VALUES ('20261022095819_InitialCreate', '10.0.13');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022130349_AddESealPermission')
BEGIN
    INSERT INTO `permissions` (`id`, `code`, `description`)
    VALUES ('8ac78524-d7b8-861f-9978-2a67584e06a8', 'eseal.view', 'Retrieve e-sealed documents by SANAD QR code');
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261022130349_AddESealPermission')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
    VALUES ('20261022130349_AddESealPermission', '10.0.13');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261024082229_AddPasswordPolicy')
BEGIN
    CREATE TABLE `password_policies` (
        `id` char(36) NOT NULL,
        `min_length` int NOT NULL,
        `max_length` int NOT NULL,
        `require_uppercase` tinyint(1) NOT NULL,
        `require_lowercase` tinyint(1) NOT NULL,
        `require_digit` tinyint(1) NOT NULL,
        `require_non_alphanumeric` tinyint(1) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `created_by` char(36) NULL,
        `updated_at` datetime(6) NULL,
        `updated_by` char(36) NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261024082229_AddPasswordPolicy')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
    VALUES ('20261024082229_AddPasswordPolicy', '10.0.13');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261024123819_AddLicense')
BEGIN
    CREATE TABLE `licenses` (
        `id` char(36) NOT NULL,
        `key` varchar(2000) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `created_by` char(36) NULL,
        `updated_at` datetime(6) NULL,
        `updated_by` char(36) NULL,
        PRIMARY KEY (`id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `migration_id` = '20261024123819_AddLicense')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
    VALUES ('20261024123819_AddLicense', '10.0.13');
END;

COMMIT;

