# Database Setup Scripts

This directory contains SQL scripts for setting up database users and schema-level permissions.

## Scripts

### setup-schema-user.sql

Creates a schema-specific PostgreSQL user with CRUD permissions on a specific schema. This script is idempotent and can be run multiple times safely.

**Features:**
- Creates schema if it doesn't exist
- Creates user if it doesn't exist (or updates password if it exists)
- Grants CRUD permissions (SELECT, INSERT, UPDATE, DELETE) on all tables
- Grants permissions on sequences (for auto-increment columns)
- Grants execute permissions on functions
- Sets up default privileges for future objects

**Usage:**

```bash
psql -h <server> -U <admin> -d <database> \
  -v schema_name='<schema>' \
  -v username='<user>' \
  -v password='<password>' \
  -f setup-schema-user.sql
```

**Example:**

```bash
psql -h myserver.postgres.database.azure.com -U dbadmin -d bowlneba \
  -v schema_name='website' \
  -v username='website_user' \
  -v password='securepassword123' \
  -f setup-schema-user.sql
```

## Deployment

These scripts are automatically executed during infrastructure deployment via GitHub Actions. See `.github/workflows/deploy_infrastructure.yml` for the automated setup process.

## Adding New Schema Users

To add a new schema-specific user:

1. Add environment variables to GitHub Actions:
   - `DATABASE_<PURPOSE>_USERNAME` (e.g., `DATABASE_REPORTS_USERNAME`)
   - `DATABASE_<PURPOSE>_PASSWORD` (secret)

2. Update the workflow to:
   - Create the connection string and store it in Key Vault
   - Run the setup script with the new schema name and credentials

3. The script will automatically:
   - Create the schema if needed
   - Create the user with proper permissions
   - Set up default privileges for future objects
