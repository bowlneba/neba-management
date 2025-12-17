-- create_users.sql
-- Initializes local dev user and schema for `neba-web` access
BEGIN;

-- Create schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS website;

-- Create role/user `neba-web` if it doesn't already exist
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'neba-web') THEN
        CREATE ROLE "neba-web" WITH LOGIN PASSWORD 'neba-web';
    END IF;
END
$$;

-- Ensure the user has usage and create privileges on the schema
GRANT USAGE ON SCHEMA website TO "neba-web";
GRANT CREATE ON SCHEMA website TO "neba-web";

-- Grant privileges on existing objects in the schema (if any)
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA website TO "neba-web";
GRANT USAGE ON ALL SEQUENCES IN SCHEMA website TO "neba-web";

-- Make future objects created by the primary DB owner (`neba`) available to `neba-web`
-- Note: the Docker Postgres container initializes objects as the POSTGRES_USER (neba)
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website GRANT ALL ON TABLES TO "neba-web";
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website GRANT USAGE ON SEQUENCES TO "neba-web";

COMMIT;

-- Local-dev credentials: username = neba-web, password = neba-web
