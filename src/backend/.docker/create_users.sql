-- create_users.sql
-- Initializes local dev users and schemas for `neba-web` and `neba-jobs` access
BEGIN;

-- Create schemas if they don't exist
CREATE SCHEMA IF NOT EXISTS website;
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Create role/user `neba-web` if it doesn't already exist
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'neba-web') THEN
        CREATE ROLE "neba-web" WITH LOGIN PASSWORD 'neba-web';
    END IF;
END
$$;

-- Create role/user `neba-jobs` if it doesn't already exist
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'neba-jobs') THEN
        CREATE ROLE "neba-jobs" WITH LOGIN PASSWORD 'neba-jobs';
    END IF;
END
$$;

-- Create role/user `neba-cache` if it doesn't already exist
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'neba-cache') THEN
        CREATE ROLE "neba-cache" WITH LOGIN PASSWORD 'neba-cache';
    END IF;
END
$$;
-- Create role/user `neba-cache` if it doesn't already exist
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'neba-cache') THEN
        CREATE ROLE "neba-cache" WITH LOGIN PASSWORD 'neba-cache';
    END IF;
END
$$;
-- Set ownership of hangfire schema to neba-jobs
ALTER SCHEMA hangfire OWNER TO "neba-jobs";

-- Grant database-level privileges to neba-web
GRANT CONNECT ON DATABASE bowlneba TO "neba-web";
GRANT TEMPORARY ON DATABASE bowlneba TO "neba-web";

-- Grant database-level privileges to neba-jobs
GRANT CONNECT ON DATABASE bowlneba TO "neba-jobs";
GRANT TEMPORARY ON DATABASE bowlneba TO "neba-jobs";

-- Grant database-level privileges to neba-cache
GRANT CONNECT ON DATABASE bowlneba TO "neba-cache";
GRANT TEMPORARY ON DATABASE bowlneba TO "neba-cache";

-- Grant database-level privileges to neba-cache
GRANT CONNECT ON DATABASE bowlneba TO "neba-cache";
GRANT TEMPORARY ON DATABASE bowlneba TO "neba-cache";

-- Grant schema-level privileges
GRANT USAGE, CREATE ON SCHEMA website TO "neba-web";

-- Grant schema-level privileges
GRANT USAGE, CREATE ON SCHEMA public TO "neba-cache";

-- Grant privileges on existing objects in the schema
GRANT SELECT, INSERT, UPDATE, DELETE, REFERENCES ON ALL TABLES IN SCHEMA website TO "neba-web";
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA website TO "neba-web";
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA website TO "neba-web";
GRANT EXECUTE ON ALL PROCEDURES IN SCHEMA website TO "neba-web";

-- Grant privileges on existing objects in the schema
GRANT SELECT, INSERT, UPDATE, DELETE, REFERENCES ON ALL TABLES IN SCHEMA public TO "neba-cache";
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO "neba-cache";
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO "neba-cache";
GRANT EXECUTE ON ALL PROCEDURES IN SCHEMA public TO "neba-cache";

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
-- Tables
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE, REFERENCES ON TABLES TO "neba-cache";

-- Sequences
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA public
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO "neba-cache";

-- Functions
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA public
    GRANT EXECUTE ON FUNCTIONS TO "neba-cache";

-- Procedures
ALTER DEFAULT PRIVILEGES FOR ROLE neba IN SCHEMA public
    GRANT EXECUTE ON ROUTINES TO "neba-cache";
-- Website: username = neba-web, password = neba-web
-- Hangfire: username = neba-jobs, password = neba-jobs
-- Cache: username = neba-cache, password = neba-cache

COMMIT;

-- Local-dev credentials: username = neba-web, password = neba-web
