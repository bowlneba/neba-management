-- PostgreSQL User Setup for Managed Identity
-- This script creates a PostgreSQL role for the API's managed identity
-- and grants appropriate permissions

-- Variable: api_app_name (the name of the API App Service)
-- This should be passed via psql -v api_app_name="app-name"

\set QUIET on
\set ON_ERROR_STOP on

-- Display setup info
\echo ''
\echo '=========================================='
\echo 'Creating PostgreSQL user for managed identity'
\echo '=========================================='
\echo ''

-- Step 1: Create role for the managed identity
\echo 'Step 1: Creating role...'
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = :'api_app_name') THEN
        EXECUTE format('CREATE ROLE %I WITH LOGIN', :'api_app_name');
        RAISE NOTICE 'Role % created', :'api_app_name';
    ELSE
        RAISE NOTICE 'Role % already exists, skipping creation', :'api_app_name';
    END IF;
END
$$;

\echo '✓ Role created'
\echo ''

-- Step 2: Grant connect permission
\echo 'Step 2: Granting database permissions...'
GRANT CONNECT ON DATABASE bowlneba TO :api_app_name;
\echo '✓ Database CONNECT granted'

-- Step 3: Grant schema usage
\echo ''
\echo 'Step 3: Granting schema permissions...'
GRANT USAGE ON SCHEMA website TO :api_app_name;
\echo '✓ Schema USAGE granted'

-- Step 4: Grant table permissions
\echo ''
\echo 'Step 4: Granting table permissions...'
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA website TO :api_app_name;
\echo '✓ Table permissions granted'

-- Step 5: Grant sequence permissions
\echo ''
\echo 'Step 5: Granting sequence permissions...'
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA website TO :api_app_name;
\echo '✓ Sequence permissions granted'

-- Step 6: Set default privileges for future objects
\echo ''
\echo 'Step 6: Setting default privileges...'
ALTER DEFAULT PRIVILEGES FOR ROLE nebaadmin IN SCHEMA website
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :api_app_name;
ALTER DEFAULT PRIVILEGES FOR ROLE nebaadmin IN SCHEMA website
    GRANT USAGE, SELECT ON SEQUENCES TO :api_app_name;
\echo '✓ Default privileges configured'

-- Verify the setup
\echo ''
\echo '=========================================='
\echo 'Verification'
\echo '=========================================='
\echo ''

\echo 'Role details:'
SELECT
    rolname as "Role Name",
    rolcanlogin as "Can Login",
    rolconnlimit as "Connection Limit"
FROM pg_roles
WHERE rolname = :'api_app_name';

\echo ''
\echo 'Database privileges:'
SELECT
    datname as "Database",
    array_agg(privilege_type) as "Privileges"
FROM information_schema.role_table_grants
WHERE grantee = :'api_app_name'
  AND table_schema = 'website'
GROUP BY datname;

\echo ''
\echo '✓ Setup completed successfully!'
\echo ''
