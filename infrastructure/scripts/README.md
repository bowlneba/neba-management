# Deployment Scripts

This directory contains scripts for setting up and managing the NEBA Management database infrastructure.

## Database Schema User Setup

The database uses schema-specific users with limited permissions for security and isolation. Each application component (website, API, reports, etc.) has its own database user with access only to their specific schema.

### How It Works

The infrastructure deployment automatically creates schema-specific users during the GitHub Actions workflow:

1. **Bicep deployment** (automated):
   - Creates PostgreSQL server with admin credentials
   - Enables SSL/TLS for secure connections
   - Sets up Key Vault for secret storage

2. **Post-deployment setup** (automated in GitHub Actions):
   - Creates connection strings in Key Vault
   - Runs SQL scripts to create schema-specific users
   - Grants appropriate CRUD permissions per schema

### Setup Script

Located in `infrastructure/database/setup-schema-user.sql`:

- Creates schema if it doesn't exist
- Creates PostgreSQL user with password
- Grants CRUD permissions (SELECT, INSERT, UPDATE, DELETE)
- Sets up permissions for sequences and functions
- Configures default privileges for future objects

**This script is idempotent** - it can be run multiple times safely without errors.

### Current Users

- **Admin User** (`DATABASE_ADMIN_USERNAME`): Full database access for migrations
- **Website User** (`DATABASE_WEB_USERNAME`): CRUD access to "website" schema only

### Connection Strings in Key Vault

After deployment, the following connection strings are available:

- `ConnectionStrings--website-migrations`: Admin credentials for running EF Core migrations
- `ConnectionStrings--website`: Website user credentials (limited to "website" schema)

### Adding New Schema-Specific Users

To add a new user for a different component (e.g., reports):

1. Add environment variables in GitHub Actions:
   - `DATABASE_<PURPOSE>_USERNAME` (e.g., `DATABASE_REPORTS_USERNAME`)
   - `DATABASE_<PURPOSE>_PASSWORD` (secret)

2. Update `.github/workflows/deploy_infrastructure.yml`:
   - Add the new secret to the workflow inputs
   - Create the connection string in Key Vault
   - Run the setup script with the new schema name

Example for a reports user:

```yaml
- name: Setup Reports Database User
  env:
    DATABASE_ADMIN_PASSWORD: ${{ secrets.DATABASE_ADMIN_PASSWORD }}
    DATABASE_REPORTS_PASSWORD: ${{ secrets.DATABASE_REPORTS_PASSWORD }}
  run: |
    PGPASSWORD="$DATABASE_ADMIN_PASSWORD" psql \
      "host=$DB_SERVER port=5432 dbname=$DB_NAME user=$ADMIN_USERNAME sslmode=require" \
      -v schema_name='reports' \
      -v username="${{ vars.DATABASE_REPORTS_USERNAME }}" \
      -v password="$DATABASE_REPORTS_PASSWORD" \
      -f infrastructure/database/setup-schema-user.sql
```

### Security Considerations

1. **Least Privilege**: Each user has access only to their specific schema
2. **No Superuser Access**: Application users cannot modify database structure
3. **Password Authentication**: Traditional PostgreSQL username/password authentication
4. **SSL/TLS Required**: All connections must use encrypted connections
5. **Key Vault Storage**: Credentials stored securely in Azure Key Vault

### Local Development

For local development with Docker:

```bash
# The docker-compose setup creates users automatically
docker compose up -d

# Connection string for local development
Host=localhost;Port=19630;Database=bowlneba;Username=website_user;Password=website_pass;
```

See `src/backend/docker-compose.yaml` for local user configuration.

### Manual Setup (if needed)

To manually create a schema user:

```bash
# Set variables
export DB_SERVER="your-server.postgres.database.azure.com"
export DB_NAME="bowlneba"
export ADMIN_USER="adminuser"
export ADMIN_PASSWORD="your-admin-password"
export SCHEMA_NAME="reports"
export NEW_USERNAME="reports_user"
export NEW_PASSWORD="secure-password"

# Run the setup script
PGPASSWORD="$ADMIN_PASSWORD" psql \
  "host=$DB_SERVER port=5432 dbname=$DB_NAME user=$ADMIN_USER sslmode=require" \
  -v schema_name="$SCHEMA_NAME" \
  -v username="$NEW_USERNAME" \
  -v password="$NEW_PASSWORD" \
  -f infrastructure/database/setup-schema-user.sql
```

### Troubleshooting

#### Error: "psql: command not found"

Install PostgreSQL client tools:
- macOS: `brew install postgresql@17`
- Ubuntu/Debian: `apt-get install postgresql-client`
- Windows: Download from [postgresql.org](https://www.postgresql.org/download/)

#### Error: "Access denied"

- Verify you're using the correct admin credentials
- Ensure the PostgreSQL server firewall allows your IP
- Check that SSL mode is set to "require"

#### Error: "Role already exists"

This is normal if re-running the script. The script updates the password but doesn't fail.

### Related Documentation

- [Database Setup Scripts](../database/README.md)
- [Infrastructure Deployment](.github/workflows/deploy_infrastructure.yml)
- [Connection String Configuration](../README.md#database-connections)


### Prerequisites

1. **Azure CLI**: Installed and authenticated (`az login`)
2. **PostgreSQL Client Tools**: `psql` command must be available
   - macOS: `brew install postgresql@17`
   - Ubuntu/Debian: `apt-get install postgresql-client`
   - Windows: Download from [postgresql.org](https://www.postgresql.org/download/)
3. **Azure Permissions**: You need appropriate permissions to:
   - Modify PostgreSQL server settings
   - Create AD admin principals
   - Connect to the database

### Files

Located in `infrastructure/scripts/`:

- **`setup-postgres-managed-identity.sh`**: Main setup script that coordinates the entire process
- **`setup-postgres-user.sql`**: SQL script that creates the database user and grants permissions

### Usage

#### Automated Setup (Recommended)

Run the main setup script with required environment variables (from the project root):

```bash
export RESOURCE_GROUP="rg-bowlneba"
export POSTGRES_SERVER_NAME="nebamgmt-eastus2"
export API_APP_NAME="app-bowlneba-api-eastus2"
export AD_ADMIN_EMAIL="your-email@example.com"
export AD_ADMIN_OBJECT_ID="your-ad-object-id"
export DATABASE_NAME="bowlneba"  # Optional, defaults to "bowlneba"

./infrastructure/scripts/setup-postgres-managed-identity.sh
```

To get your Azure AD object ID:

```bash
az ad signed-in-user show --query id -o tsv
```

#### Manual Setup

If you prefer to run steps manually:

1. **Enable Azure AD authentication:**

   ```bash
   az postgres flexible-server update \
       --resource-group rg-bowlneba \
       --name nebamgmt-eastus2 \
       --active-directory-auth Enabled
   ```

2. **Set Azure AD admin:**

   ```bash
   az postgres flexible-server ad-admin create \
       --resource-group rg-bowlneba \
       --server-name nebamgmt-eastus2 \
       --object-id <your-object-id> \
       --display-name "your-email@example.com" \
       --type User
   ```

3. **Run SQL setup:**

   ```bash
   # Get access token
   TOKEN=$(az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv)

   # Run SQL script
   PGPASSWORD="$TOKEN" psql \
       "host=nebamgmt-eastus2.postgres.database.azure.com port=5432 dbname=bowlneba user=your-email@example.com sslmode=require" \
       -v api_app_name="app-bowlneba-api-eastus2" \
       -f infrastructure/scripts/setup-postgres-user.sql
   ```

### What the Scripts Do

1. **Set AD Admin**: Configures an Azure AD user as the database admin (Azure AD auth is already enabled by Bicep)
2. **Create Database Role**: Creates a PostgreSQL role for the API's managed identity
3. **Grant Permissions**: Grants appropriate database, schema, table, and sequence permissions
4. **Set Default Privileges**: Configures default privileges for future database objects

**Note**: The Bicep infrastructure already enables Azure AD authentication on the server. These scripts complete the setup by configuring the admin and database-level permissions.

### Connection String Configuration

After running these scripts, update your Key Vault connection string to use managed identity authentication:

#### For Password Authentication (Current)

```text
Host=server.postgres.database.azure.com;Database=bowlneba;Username=nebaadmin;Password=<password>;SslMode=Require
```

#### For Managed Identity Authentication (Future)

```text
Host=server.postgres.database.azure.com;Database=bowlneba;Username=app-name;SslMode=Require
```

Note: When using managed identity, the application code needs to obtain an Azure AD access token and use it as the password. This requires additional application changes.

### Integration with Deployment Pipeline

To integrate these scripts into your GitHub Actions or Azure DevOps pipeline:

```yaml
- name: Setup PostgreSQL Managed Identity
  run: |
    export RESOURCE_GROUP="${{ secrets.AZURE_RESOURCE_GROUP }}"
    export POSTGRES_SERVER_NAME="${{ secrets.POSTGRES_SERVER_NAME }}"
    export API_APP_NAME="${{ secrets.API_APP_NAME }}"
    export AD_ADMIN_EMAIL="${{ secrets.AD_ADMIN_EMAIL }}"
    export AD_ADMIN_OBJECT_ID="${{ secrets.AD_ADMIN_OBJECT_ID }}"
    ./infrastructure/scripts/setup-postgres-managed-identity.sh
```

### Troubleshooting

#### Error: "psql: command not found"

- Install PostgreSQL client tools (see Prerequisites)

#### Error: "Failed to get managed identity"

- Ensure the API App Service has a system-assigned managed identity enabled
- Check that you're using the correct app name

#### Error: "Access denied" or authentication failures

- Verify you're logged into the correct Azure subscription
- Ensure your Azure AD account has permissions on the PostgreSQL server
- Try re-authenticating: `az login`

#### Error: "Role already exists"

- The script is idempotent and will skip creating the role if it already exists
- This is normal for re-runs

### Security Considerations

1. **Managed Identity**: More secure than password-based authentication as there are no credentials to manage
2. **Azure AD Integration**: Centralized access control through Azure AD
3. **Least Privilege**: The database user is granted only necessary permissions (no superuser access)
4. **No Hardcoded Credentials**: Access tokens are obtained dynamically and expire automatically

### Infrastructure Configuration

The Bicep infrastructure enables Azure AD authentication by default:

**Module: `infrastructure/modules/postgreSqlFlexibleServer.bicep`**

- `enableAzureADAuth` parameter (default: `true`) - Enables Azure AD auth capability
- `enablePasswordAuth` parameter (default: `true`) - Keeps password auth for compatibility
- `authConfig` property - Configures both authentication methods

**What this means:**

- New environments can use password auth immediately after Bicep deployment
- Azure AD auth is available when you're ready (run these scripts to activate it)
- Both methods can coexist during migration

### Deployment Workflow

**For new environments:**

1. **Deploy infrastructure** (Bicep):

   ```bash
   az deployment sub create --location eastus2 --template-file infrastructure/main.bicep ...
   ```

   → Environment is ready with **password authentication**

2. **Optional: Enable managed identity** (these scripts):

   ```bash
   ./infrastructure/scripts/setup-postgres-managed-identity.sh
   ```

   → Environment can now use **managed identity authentication**

**You only need to run these scripts if you want to use managed identity authentication instead of passwords.**
