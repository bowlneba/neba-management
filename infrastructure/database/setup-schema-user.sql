-- Setup script for schema-specific database users
-- This script is idempotent and can be run multiple times safely
--
-- Usage:
--   psql -h <server> -U <admin> -d <database> -v schema_name='<schema>' -v username='<user>' -v password='<password>' -f setup-schema-user.sql
--
-- Variables:
--   :schema_name - The schema to create and grant access to
--   :username - The PostgreSQL user to create
--   :password - The password for the user

-- Create schema if it doesn't exist
DO $$
BEGIN
    EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', :'schema_name');
END
$$;

-- Create user if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = :'username') THEN
        EXECUTE format('CREATE USER %I WITH PASSWORD %L', :'username', :'password');
        RAISE NOTICE 'User % created', :'username';
    ELSE
        -- Update password in case it changed
        EXECUTE format('ALTER USER %I WITH PASSWORD %L', :'username', :'password');
        RAISE NOTICE 'User % already exists, password updated', :'username';
    END IF;
END
$$;

-- Grant privileges using dynamic SQL to safely handle identifiers
DO $$
DECLARE
    schema_name TEXT := :'schema_name';
    username TEXT := :'username';
BEGIN
    -- Grant schema usage
    EXECUTE format(
        'GRANT USAGE ON SCHEMA %I TO %I',
        schema_name,
        username
    );

    -- Grant CRUD permissions on all existing tables
    EXECUTE format(
        'GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA %I TO %I',
        schema_name,
        username
    );

    -- Grant CRUD permissions on all future tables
    EXECUTE format(
        'ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO %I',
        schema_name,
        username
    );

    -- Grant usage on all sequences (for auto-increment columns)
    EXECUTE format(
        'GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA %I TO %I',
        schema_name,
        username
    );

    -- Grant usage on all future sequences
    EXECUTE format(
        'ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT USAGE, SELECT ON SEQUENCES TO %I',
        schema_name,
        username
    );

    -- Grant execute on all functions (if needed for stored procedures)
    EXECUTE format(
        'GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA %I TO %I',
        schema_name,
        username
    );

    -- Grant execute on all future functions
    EXECUTE format(
        'ALTER DEFAULT PRIVILEGES IN SCHEMA %I GRANT EXECUTE ON FUNCTIONS TO %I',
        schema_name,
        username
    );
END
$$;

-- Verify permissions
-- Note: The verification queries below use psql variable substitution (:'variable')
-- which substitutes the variable as a quoted string literal. This is safe for
-- WHERE clauses in SELECT statements. For DDL/DML operations, we use EXECUTE format()
-- with %I/%L for proper identifier/literal quoting (see DO block above).
\echo 'Schema and user setup completed successfully'
\echo 'Verifying permissions...'
SELECT
    nspname AS schema_name,
    pg_catalog.pg_get_userbyid(nspowner) AS schema_owner
FROM pg_catalog.pg_namespace
WHERE nspname = :'schema_name';

\echo 'User roles:'
SELECT
    rolname,
    rolsuper,
    rolinherit,
    rolcreaterole,
    rolcreatedb,
    rolcanlogin
FROM pg_catalog.pg_roles
WHERE rolname = :'username';
