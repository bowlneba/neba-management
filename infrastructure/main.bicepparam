// Parameters file for local testing and reference
// In GitHub Actions, these will be passed as arguments instead
// This is similar to Terraform's .tfvars file

using './main.bicep'

// Example values for development environment
param azureResourceGroupName = 'rg-neba-dev'
param azureLocation = 'eastus'
param azureEnvironment = 'dev'
param azureAppServicePlanName = 'asp-neba-dev'
param azureApiAppServiceName = 'app-neba-api-dev'
param azureWebAppServiceName = 'app-neba-web-dev'
param azureAppServicePlanSku = 'B1'
param azureStorageAccountName = 'stnebadev'

param tags = {
  Environment: 'dev'
  ManagedBy: 'Bicep'
  Project: 'NebaManagement'
}
