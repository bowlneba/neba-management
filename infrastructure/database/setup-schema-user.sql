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
CREATE SCHEMA IF NOT EXISTS :schema_name;

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

-- Grant schema usage
GRANT USAGE ON SCHEMA :schema_name TO :username;

-- Grant CRUD permissions on all existing tables
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA :schema_name TO :username;

-- Grant CRUD permissions on all future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA :schema_name GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :username;

-- Grant usage on all sequences (for auto-increment columns)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA :schema_name TO :username;

-- Grant usage on all future sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA :schema_name GRANT USAGE, SELECT ON SEQUENCES TO :username;

-- Grant execute on all functions (if needed for stored procedures)
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA :schema_name TO :username;

-- Grant execute on all future functions
ALTER DEFAULT PRIVILEGES IN SCHEMA :schema_name GRANT EXECUTE ON FUNCTIONS TO :username;

-- Verify permissions
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
