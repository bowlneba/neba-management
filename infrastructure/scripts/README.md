# Deployment Scripts

This directory contains scripts for setting up and managing the NEBA Management infrastructure.

## PostgreSQL Managed Identity Setup

These scripts complete the Azure AD authentication setup for managed identity database access.

### How It Works

The infrastructure (Bicep) enables Azure AD authentication on the PostgreSQL server, but additional steps are required to actually use it:

1. **Bicep deployment** (automated):
   - Enables Azure AD authentication capability on the server
   - Enables password authentication (for backward compatibility)

2. **Post-deployment setup** (these scripts):
   - Sets an Azure AD admin user
   - Creates a database role for the API's managed identity
   - Grants appropriate permissions

**Without running these scripts**, your environment will use password-based authentication (which works immediately after Bicep deployment).

**After running these scripts**, your environment can use managed identity authentication (more secure, no password needed).

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
