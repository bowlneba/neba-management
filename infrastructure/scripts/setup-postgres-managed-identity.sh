#!/bin/bash
set -e

# Setup PostgreSQL Managed Identity Authentication
# This script enables Azure AD authentication on the PostgreSQL server
# and creates a database user for the API's managed identity

# Required environment variables:
# - RESOURCE_GROUP: Azure resource group name
# - POSTGRES_SERVER_NAME: PostgreSQL server name
# - API_APP_NAME: API App Service name
# - AD_ADMIN_EMAIL: Email of the Azure AD admin user
# - AD_ADMIN_OBJECT_ID: Object ID of the Azure AD admin user
# - DATABASE_NAME: Database name (default: bowlneba)

RESOURCE_GROUP="${RESOURCE_GROUP:?Required: RESOURCE_GROUP}"
POSTGRES_SERVER_NAME="${POSTGRES_SERVER_NAME:?Required: POSTGRES_SERVER_NAME}"
API_APP_NAME="${API_APP_NAME:?Required: API_APP_NAME}"
AD_ADMIN_EMAIL="${AD_ADMIN_EMAIL:?Required: AD_ADMIN_EMAIL}"
AD_ADMIN_OBJECT_ID="${AD_ADMIN_OBJECT_ID:?Required: AD_ADMIN_OBJECT_ID}"
DATABASE_NAME="${DATABASE_NAME:-bowlneba}"

echo "=========================================="
echo "PostgreSQL Managed Identity Setup"
echo "=========================================="
echo "Resource Group: $RESOURCE_GROUP"
echo "PostgreSQL Server: $POSTGRES_SERVER_NAME"
echo "API App: $API_APP_NAME"
echo "Database: $DATABASE_NAME"
echo "AD Admin: $AD_ADMIN_EMAIL"
echo "=========================================="

# Step 1: Enable Azure AD authentication on PostgreSQL
echo ""
echo "Step 1: Enabling Azure AD authentication on PostgreSQL server..."
az postgres flexible-server update \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_SERVER_NAME" \
    --active-directory-auth Enabled \
    --output none

echo "✓ Azure AD authentication enabled"

# Step 2: Set Azure AD admin
echo ""
echo "Step 2: Setting Azure AD admin..."
az postgres flexible-server ad-admin create \
    --resource-group "$RESOURCE_GROUP" \
    --server-name "$POSTGRES_SERVER_NAME" \
    --object-id "$AD_ADMIN_OBJECT_ID" \
    --display-name "$AD_ADMIN_EMAIL" \
    --type User \
    --output none

echo "✓ Azure AD admin configured"

# Step 3: Get API managed identity principal ID
echo ""
echo "Step 3: Getting API managed identity..."
API_PRINCIPAL_ID=$(az webapp identity show \
    --name "$API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query principalId \
    --output tsv)

if [ -z "$API_PRINCIPAL_ID" ]; then
    echo "ERROR: Failed to get managed identity for $API_APP_NAME"
    echo "Make sure the API app has a system-assigned managed identity enabled"
    exit 1
fi

echo "✓ API managed identity: $API_PRINCIPAL_ID"

# Step 4: Create database user for managed identity
echo ""
echo "Step 4: Creating database user for managed identity..."
echo "This requires psql to be installed and accessible"

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo "ERROR: psql command not found"
    echo "Please install PostgreSQL client tools:"
    echo "  - macOS: brew install postgresql@17"
    echo "  - Ubuntu/Debian: apt-get install postgresql-client"
    echo "  - Windows: Download from postgresql.org"
    exit 1
fi

# Get PostgreSQL FQDN
POSTGRES_FQDN=$(az postgres flexible-server show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_SERVER_NAME" \
    --query fullyQualifiedDomainName \
    --output tsv)

echo "PostgreSQL FQDN: $POSTGRES_FQDN"

# Get Azure AD access token for PostgreSQL
echo "Getting Azure AD access token..."
ACCESS_TOKEN=$(az account get-access-token --resource-type oss-rdbms --query accessToken --output tsv)

if [ -z "$ACCESS_TOKEN" ]; then
    echo "ERROR: Failed to get Azure AD access token"
    exit 1
fi

# Execute SQL script to create user and grant permissions
echo "Executing SQL script to create user and grant permissions..."
PGPASSWORD="$ACCESS_TOKEN" psql \
    "host=$POSTGRES_FQDN port=5432 dbname=$DATABASE_NAME user=$AD_ADMIN_EMAIL sslmode=require" \
    -v api_app_name="$API_APP_NAME" \
    -f "$(dirname "$0")/setup-postgres-user.sql"

echo ""
echo "=========================================="
echo "✓ Setup completed successfully!"
echo "=========================================="
echo ""
echo "The API app '$API_APP_NAME' can now authenticate to PostgreSQL"
echo "using its managed identity without a password."
echo ""
echo "Next steps:"
echo "1. Update the connection string in Key Vault to use managed identity authentication"
echo "2. Restart the API app to apply changes"
