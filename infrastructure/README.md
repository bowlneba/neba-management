# NEBA Management Infrastructure

This folder contains the Infrastructure as Code (IaC) for deploying NEBA Management to Azure using **Bicep**.

## 📁 Structure

```
infrastructure/
├── main.bicep              # Entry point - defines all resources
├── main.bicepparam         # Example parameters for local testing
└── modules/
    ├── appServicePlan.bicep  # App Service Plan module
    └── appService.bicep      # App Service module (reusable for API & Web)
```

## 🔄 Bicep vs Terraform Key Differences

Coming from Terraform, here are the main differences:

| Concept | Terraform | Bicep |
|---------|-----------|-------|
| **Language** | HCL (HashiCorp Configuration Language) | Bicep (Azure-specific DSL) |
| **Provider** | `azurerm` provider needed | Native Azure - no provider config |
| **State** | External state file (local/remote) | Managed by Azure automatically |
| **Resources** | `resource "azurerm_..."` | `resource name 'Microsoft.../...'` |
| **Modules** | `module "name" { source = "..." }` | `module name 'path/to/file.bicep'` |
| **Variables** | `variable "name" {}` | `param name type` |
| **Outputs** | `output "name" { value = ... }` | `output name type = value` |
| **Scope** | Implicit resource group | Explicit with `targetScope` |

### Key Bicep Concepts

1. **`targetScope`**: Defines deployment level
   - `subscription` - Create resource groups
   - `resourceGroup` - Deploy within an RG (default)
   - `managementGroup` - Enterprise-level
   - `tenant` - Tenant-wide

2. **Module Scope**: Unlike Terraform, you explicitly set the scope:

   ```bicep
   module myModule 'module.bicep' = {
     scope: rg  // Deploy into this resource group
     name: 'deployment-name'
     params: { ... }
   }
   ```

3. **No State Files**: Azure Resource Manager tracks state automatically - no `.tfstate` files!

4. **API Versions**: Resources require specific API versions (e.g., `@2023-12-01`)

## 🚀 Local Deployment (Testing)

### Prerequisites

Install Azure CLI and Bicep:

```bash
# Install/update Azure CLI
brew install azure-cli

# Bicep is included, but update to latest
az bicep upgrade

# Login to Azure
az login
```

### Deploy with Parameter File

```bash
# Deploy to a subscription
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters main.bicepparam
```

### Deploy with Inline Parameters

```bash
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters \
    azureResourceGroupName='rg-neba-dev' \
    azureLocation='eastus' \
    azureEnvironment='dev' \
    azureAppServicePlanName='asp-neba-dev' \
    azureApiAppServiceName='app-neba-api-dev' \
    azureWebAppServiceName='app-neba-web-dev' \
    azureAppServicePlanSku='B1'
```

## 🔍 Validation & What-If

```bash
# Validate syntax
az bicep build --file main.bicep

# Preview changes (like terraform plan)
az deployment sub what-if \
  --location eastus \
  --template-file main.bicep \
  --parameters main.bicepparam
```

## 📦 GitHub Actions Integration

Parameters will be injected from GitHub environment variables:

```yaml
# Example GitHub Actions step (you'll create this later)
- name: Deploy Infrastructure
  run: |
    az deployment sub create \
      --location ${{ vars.AZURE_LOCATION }} \
      --template-file infrastructure/main.bicep \
      --parameters \
        azureResourceGroupName='${{ vars.AZURE_RESOURCE_GROUP_NAME }}' \
        azureLocation='${{ vars.AZURE_LOCATION }}' \
        azureEnvironment='${{ vars.AZURE_ENVIRONMENT }}' \
        azureAppServicePlanName='${{ vars.AZURE_APP_SERVICE_PLAN_NAME }}' \
        azureApiAppServiceName='${{ vars.AZURE_API_APP_SERVICE_NAME }}' \
        azureWebAppServiceName='${{ vars.AZURE_WEB_APP_SERVICE_NAME }}' \
        azureAppServicePlanSku='${{ vars.AZURE_APP_SERVICE_PLAN_SKU }}'
```

## 📝 Current Infrastructure

**Phase 1: Core Compute** ✅

- Resource Group
- App Service Plan (Linux, B1 SKU)
- API App Service (.NET 8)
- Web App Service (.NET 8, Blazor)

**Phase 2: Coming Soon** 🚧

- Azure Database for PostgreSQL (or SQL Server)
- Azure Cosmos DB for MongoDB API
- Application Insights
- Azure Key Vault
- Azure Storage (Blobs)

## 🔐 Security Notes

- All App Services enforce HTTPS only
- TLS 1.2 minimum
- FTPS disabled
- App settings will be enhanced with Key Vault references in Phase 2

## 🎯 Naming Convention

Resources follow Azure best practices:

- `rg-{app}-{env}` - Resource Group
- `asp-{app}-{env}` - App Service Plan
- `app-{app}-{component}-{env}` - App Service

Example: `app-neba-api-dev`, `app-neba-web-prod`

## 📚 Useful Commands

```bash
# List all deployments in a subscription
az deployment sub list --output table

# Get deployment outputs
az deployment sub show \
  --name <deployment-name> \
  --query properties.outputs

# Delete a resource group
az group delete --name rg-neba-dev --yes
```

## 🆘 Troubleshooting

### Error: "Scope is not valid"

- Make sure `targetScope = 'subscription'` is at the top of `main.bicep`

### Error: "Resource already exists"

- Bicep is idempotent - re-running updates existing resources
- Check if names are unique globally (App Service names must be globally unique)

### Error: "API version is invalid"

- Update the API version in the resource declaration
- Find latest versions: `az provider show --namespace Microsoft.Web --query "resourceTypes[?resourceType=='sites'].apiVersions" --output table`
