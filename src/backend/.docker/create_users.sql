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

-- Grant database-level privileges
GRANT CONNECT ON DATABASE bowlneba TO "neba-web";
GRANT TEMPORARY ON DATABASE bowlneba TO "neba-web";

-- Grant schema-level privileges
GRANT USAGE, CREATE ON SCHEMA website TO "neba-web";

-- Grant privileges on existing objects in the schema
GRANT SELECT, INSERT, UPDATE, DELETE, REFERENCES ON ALL TABLES IN SCHEMA website TO "neba-web";
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA website TO "neba-web";
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA website TO "neba-web";
GRANT EXECUTE ON ALL PROCEDURES IN SCHEMA website TO "neba-web";

-- Set default privileges for future objects created by the primary DB owner (`neba`)
-- Tables
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website
    GRANT SELECT, INSERT, UPDATE, DELETE, REFERENCES ON TABLES TO "neba-web";

-- Sequences
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO "neba-web";

-- Functions
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website
    GRANT EXECUTE ON FUNCTIONS TO "neba-web";

-- Procedures
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA website
    GRANT EXECUTE ON ROUTINES TO "neba-web";

COMMIT;

-- Local-dev credentials: username = neba-web, password = neba-web
