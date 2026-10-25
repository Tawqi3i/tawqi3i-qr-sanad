CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE permissions (
        id uuid NOT NULL,
        code character varying(128) NOT NULL,
        description character varying(256) NOT NULL,
        CONSTRAINT pk_permissions PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE security_audit_logs (
        id uuid NOT NULL,
        event_type character varying(64) NOT NULL,
        user_id uuid,
        user_name character varying(256),
        actor_id uuid,
        branch_id uuid,
        ip_address character varying(64),
        user_agent character varying(512),
        details character varying(2000),
        created_at timestamp with time zone NOT NULL,
        CONSTRAINT pk_security_audit_logs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE branches (
        id uuid NOT NULL,
        name character varying(128) NOT NULL,
        normalized_name character varying(128) NOT NULL,
        code character varying(32),
        address character varying(256),
        phone_number character varying(32),
        status character varying(32) NOT NULL,
        admin_user_id uuid,
        concurrency_stamp character varying(64) NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        created_at timestamp with time zone NOT NULL,
        created_by uuid,
        updated_at timestamp with time zone,
        updated_by uuid,
        CONSTRAINT pk_branches PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        user_name character varying(64) NOT NULL,
        normalized_user_name character varying(64) NOT NULL,
        email character varying(256) NOT NULL,
        normalized_email character varying(256) NOT NULL,
        display_name character varying(128) NOT NULL,
        phone_number character varying(32),
        user_type character varying(32) NOT NULL,
        branch_id uuid,
        password_hash character varying(512) NOT NULL,
        security_stamp character varying(64) NOT NULL,
        concurrency_stamp character varying(64) NOT NULL,
        status character varying(32) NOT NULL,
        access_failed_count integer NOT NULL,
        lockout_end timestamp with time zone,
        last_login_at timestamp with time zone,
        password_changed_at timestamp with time zone,
        must_change_password boolean NOT NULL,
        is_deleted boolean NOT NULL,
        deleted_at timestamp with time zone,
        deleted_by uuid,
        created_at timestamp with time zone NOT NULL,
        created_by uuid,
        updated_at timestamp with time zone,
        updated_by uuid,
        CONSTRAINT pk_users PRIMARY KEY (id),
        CONSTRAINT fk_users_branches_branch_id FOREIGN KEY (branch_id) REFERENCES branches (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE refresh_tokens (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        token_hash character varying(128) NOT NULL,
        security_stamp character varying(64) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        created_by_ip character varying(64),
        user_agent character varying(512),
        revoked_at timestamp with time zone,
        revoked_by_ip character varying(64),
        revoke_reason character varying(64),
        replaced_by_token_id uuid,
        CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
        CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE TABLE user_permissions (
        user_id uuid NOT NULL,
        permission_id uuid NOT NULL,
        assigned_at timestamp with time zone NOT NULL,
        assigned_by uuid,
        CONSTRAINT pk_user_permissions PRIMARY KEY (user_id, permission_id),
        CONSTRAINT fk_user_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES permissions (id) ON DELETE CASCADE,
        CONSTRAINT fk_user_permissions_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    INSERT INTO permissions (id, code, description)
    VALUES ('6b5dd7de-fed2-8a5b-8064-b4d9a2551f9d', 'users.update', 'Update users, change their status and unlock them');
    INSERT INTO permissions (id, code, description)
    VALUES ('935d225d-1b6c-830d-916b-c94acbf35197', 'users.reset_password', 'Reset another user''s password');
    INSERT INTO permissions (id, code, description)
    VALUES ('9b5270e0-5623-8d12-bf45-ae8cd430b855', 'users.assign_permissions', 'Grant and revoke user permissions');
    INSERT INTO permissions (id, code, description)
    VALUES ('a1885bbe-d02b-8363-bf99-25e9c4bad98d', 'users.create', 'Create users in the branch');
    INSERT INTO permissions (id, code, description)
    VALUES ('b5b3840b-f1b5-8912-909e-80df84a42698', 'users.delete', 'Delete users');
    INSERT INTO permissions (id, code, description)
    VALUES ('c7994376-f8e1-8e70-a2f6-5de3a6ea4963', 'permissions.view', 'View the permission catalogue');
    INSERT INTO permissions (id, code, description)
    VALUES ('f969d5a8-1f29-8efb-b321-4e7cc4654082', 'audit_logs.view', 'View the security audit log of the branch');
    INSERT INTO permissions (id, code, description)
    VALUES ('faebb80f-3f91-88d3-8344-93229a4b7798', 'users.view', 'View the users of the branch');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_branches_admin_user_id ON branches (admin_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_branches_code ON branches (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_branches_normalized_name ON branches (normalized_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_permissions_code ON permissions (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_refresh_tokens_user_id_revoked_at ON refresh_tokens (user_id, revoked_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_security_audit_logs_branch_id_created_at ON security_audit_logs (branch_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_security_audit_logs_created_at ON security_audit_logs (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_security_audit_logs_user_id_created_at ON security_audit_logs (user_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_user_permissions_permission_id ON user_permissions (permission_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_users_branch_id_user_type ON users (branch_id, user_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE INDEX ix_users_created_at ON users (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_users_normalized_email ON users (normalized_email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_users_normalized_user_name ON users (normalized_user_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    ALTER TABLE branches ADD CONSTRAINT fk_branches_users_admin_user_id FOREIGN KEY (admin_user_id) REFERENCES users (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022095804_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261022095804_InitialCreate', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022130339_AddESealPermission') THEN
    INSERT INTO permissions (id, code, description)
    VALUES ('8ac78524-d7b8-861f-9978-2a67584e06a8', 'eseal.view', 'Retrieve e-sealed documents by SANAD QR code');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261022130339_AddESealPermission') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261022130339_AddESealPermission', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261024082224_AddPasswordPolicy') THEN
    CREATE TABLE password_policies (
        id uuid NOT NULL,
        min_length integer NOT NULL,
        max_length integer NOT NULL,
        require_uppercase boolean NOT NULL,
        require_lowercase boolean NOT NULL,
        require_digit boolean NOT NULL,
        require_non_alphanumeric boolean NOT NULL,
        created_at timestamp with time zone NOT NULL,
        created_by uuid,
        updated_at timestamp with time zone,
        updated_by uuid,
        CONSTRAINT pk_password_policies PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261024082224_AddPasswordPolicy') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261024082224_AddPasswordPolicy', '10.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261024123813_AddLicense') THEN
    CREATE TABLE licenses (
        id uuid NOT NULL,
        key character varying(2000) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        created_by uuid,
        updated_at timestamp with time zone,
        updated_by uuid,
        CONSTRAINT pk_licenses PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20261024123813_AddLicense') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20261024123813_AddLicense', '10.0.13');
    END IF;
END $EF$;
COMMIT;

