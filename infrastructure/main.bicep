// Main infrastructure deployment for NEBA Management
// This is the entry point - similar to Terraform's main.tf

targetScope = 'subscription'

// Parameters will be injected from GitHub Actions environment variables
@description('The name of the resource group')
param azureResourceGroupName string

@description('Azure region for all resources')
param azureLocation string

@description('Environment name (dev, staging, prod)')
@maxLength(10)
param azureEnvironment string

@description('Name of the App Service Plan')
param azureAppServicePlanName string

@description('Name of the API App Service')
param azureApiAppServiceName string

@description('Name of the Web App Service')
param azureWebAppServiceName string

@description('App Service Plan SKU. Common values: B1, B2, B3 (Basic), S1, S2, S3 (Standard), P1v3, P2v3, P3v3 (Premium v3)')
param azureAppServicePlanSku string = 'B1'

@description('PostgreSQL server name')
param databaseServerName string

@description('PostgreSQL database name')
param databaseName string

@description('PostgreSQL administrator username')
param databaseAdminUsername string

@description('PostgreSQL administrator password')
@secure()
param databaseAdminPassword string

@description('PostgreSQL SKU')
param azurePostgresSku string = 'Standard_B1ms'

@description('PostgreSQL storage size in GB')
param azurePostgresStorageSizeGB int = 32

@description('PostgreSQL version')
param postgresVersion string = '17'

@description('PostgreSQL backup retention days')
param azurePostgresBackupRetentionDays int = 7

@description('Key Vault name')
param azureKeyVaultName string

@description('Storage Account name')
param azureStorageAccountName string

@description('Azure Maps Account name')
param azureMapsAccountName string

@description('Tags to apply to all resources')
param tags object = {
  Environment: azureEnvironment
  ManagedBy: 'Bicep'
  Project: 'NebaManagement'
}

// Storage account names are limited to 24 characters and must be lowercase
var locationNoDash = replace(azureLocation, '-', '')
var allowedPrefixLength = max(0, 24 - length(locationNoDash))
var prefixLength = min(length(azureStorageAccountName), allowedPrefixLength)
var storageAccountNamePrefix = substring(azureStorageAccountName, 0, prefixLength)
var storageAccountFinalName = toLower(replace('${storageAccountNamePrefix}${locationNoDash}', '-', ''))

// Resource Group - In Bicep, we explicitly create the RG at subscription scope
resource rg 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: azureResourceGroupName
  location: azureLocation
  tags: tags
}

// App Service Plan Module
module appServicePlan 'modules/appServicePlan.bicep' = {
  scope: rg
  name: 'appServicePlan-deployment'
  params: {
    name: '${azureAppServicePlanName}-${azureLocation}'
    location: azureLocation
    sku: azureAppServicePlanSku
    tags: tags
  }
}

// Key Vault Module
module keyVault 'modules/keyVault.bicep' = {
  scope: rg
  name: 'keyVault-deployment'
  params: {
    name: '${azureKeyVaultName}-${azureLocation}'
    location: azureLocation
    skuName: 'standard'
    enableSoftDelete: true
    enablePurgeProtection: true
    enableRbacAuthorization: true
    enableAzureServicesAccess: true
    tags: union(tags, { Component: 'KeyVault' })
  }
}

// Storage Account Module
module storageAccount 'modules/storageAccount.bicep' = {
  scope: rg
  name: 'storageAccount-deployment'
  params: {
    name: storageAccountFinalName
    location: azureLocation
    skuName: 'Standard_LRS'
    accessTier: 'Hot'
    tags: union(tags, { Component: 'Storage' })
  }
}

// Azure Maps Account Module
module mapsAccount 'modules/azureMapsAccount.bicep' = {
  scope: rg
  name: 'mapsAccount-deployment'
  params: {
    name: '${azureMapsAccountName}-${azureLocation}'
    location: azureLocation
    skuName: 'G2'
    tags: union(tags, { Component: 'Maps' })
  }
}

// PostgreSQL Flexible Server Module
module postgreSqlServer 'modules/postgreSqlFlexibleServer.bicep' = {
  scope: rg
  name: 'postgreSqlServer-deployment'
  params: {
    name: '${databaseServerName}-${azureLocation}'
    location: azureLocation
    administratorLogin: databaseAdminUsername
    administratorLoginPassword: databaseAdminPassword
    version: postgresVersion
    skuName: azurePostgresSku
    storageSizeGB: azurePostgresStorageSizeGB
    backupRetentionDays: azurePostgresBackupRetentionDays
    highAvailability: false
    databaseName: databaseName
    tags: union(tags, { Component: 'Database' })
  }
}

// API App Service Module
module apiAppService 'modules/appService.bicep' = {
  scope: rg
  name: 'apiAppService-deployment'
  params: {
    name: '${azureApiAppServiceName}-${azureLocation}'
    location: azureLocation
    appServicePlanId: appServicePlan.outputs.id
    tags: union(tags, { Component: 'API' })
    corsAllowedOrigins: [
      'https://${azureWebAppServiceName}-${azureLocation}.azurewebsites.net'
    ]
    appSettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: azureEnvironment
      }
      {
        name: 'ASPNETCORE_URLS'
        value: 'http://+:8080'
      }
      {
        name: 'KeyVault__VaultUrl'
        value: keyVault.outputs.uri
      }
      {
        name: 'AzureStorage__BlobServiceUri'
        value: storageAccount.outputs.primaryBlobEndpoint
      }
    ]
  }
}

// RBAC Role Assignment: API Managed Identity -> Key Vault Secrets User
// Key Vault Secrets User role ID: 4633458b-17de-408a-b874-0445c86b69e6
module apiKeyVaultAccess 'modules/keyVaultRoleAssignment.bicep' = {
  scope: rg
  name: 'apiKeyVaultAccess-deployment'
  params: {
    keyVaultName: keyVault.outputs.name
    principalId: apiAppService.outputs.principalId
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6'
  }
}

// RBAC Role Assignment: Web Managed Identity -> Key Vault Secrets User
// Needed for Web.Server to read Hangfire connection string from Key Vault
module webKeyVaultAccess 'modules/keyVaultRoleAssignment.bicep' = {
  scope: rg
  name: 'webKeyVaultAccess-deployment'
  params: {
    keyVaultName: keyVault.outputs.name
    principalId: webAppService.outputs.principalId
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6'
  }
}

// RBAC Role Assignment: Web Managed Identity -> Azure Maps Data Reader
// Azure Maps Data Reader role ID: 423170ca-a8f6-4b0f-8487-9e4eb8f49baa
module webMapsAccess 'modules/azureMapsRoleAssignment.bicep' = {
  scope: rg
  name: 'webMapsAccess-deployment'
  params: {
    mapsAccountName: mapsAccount.outputs.name
    principalId: webAppService.outputs.principalId
    roleDefinitionId: '423170ca-a8f6-4b0f-8487-9e4eb8f49baa'
  }
}

// RBAC Role Assignment: API Managed Identity -> Storage Blob Data Contributor
// Storage Blob Data Contributor role ID: ba92f5b4-2d11-453d-a403-e96b0029c9fe
module apiStorageAccess 'modules/storageRoleAssignment.bicep' = {
  scope: rg
  name: 'apiStorageAccess-deployment'
  params: {
    storageAccountName: storageAccount.outputs.name
    principalId: apiAppService.outputs.principalId
    roleDefinitionId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  }
}

// Note: we do not create a Key Vault secret for the storage account connection string
// because account keys are not exposed by default. Prefer Managed Identity / AAD
// authentication. If you need the connection string saved, re-enable this module
// and ensure `storageAccount` outputs a secure `connectionString` (use @secure()).

// Web App Service Module
module webAppService 'modules/appService.bicep' = {
  scope: rg
  name: 'webAppService-deployment'
  params: {
    name: '${azureWebAppServiceName}-${azureLocation}'
    location: azureLocation
    appServicePlanId: appServicePlan.outputs.id
    tags: union(tags, { Component: 'Web' })
    startupCommand: 'dotnet Neba.Web.Server.dll'
    appSettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: azureEnvironment
      }
      {
        name: 'ASPNETCORE_URLS'
        value: 'http://+:8080'
      }
      {
        name: 'NebaApi__BaseUrl'
        value: 'https://${apiAppService.outputs.defaultHostName}'
      }
      {
        name: 'KeyVault__VaultUrl'
        value: keyVault.outputs.uri
      }
      {
        name: 'AzureMaps__AccountId'
        value: mapsAccount.outputs.accountId
      }
    ]
  }
}

// Outputs - similar to Terraform outputs, these can be used by GitHub Actions
output resourceGroupName string = rg.name
output appServicePlanId string = appServicePlan.outputs.id
output apiAppServiceName string = apiAppService.outputs.name
output apiAppServiceUrl string = 'https://${apiAppService.outputs.defaultHostName}'
output apiAppServicePrincipalId string = apiAppService.outputs.principalId
output webAppServiceName string = webAppService.outputs.name
output webAppServiceUrl string = 'https://${webAppService.outputs.defaultHostName}'
output webAppServicePrincipalId string = webAppService.outputs.principalId
output postgreSqlServerName string = postgreSqlServer.outputs.name
output postgreSqlServerFqdn string = postgreSqlServer.outputs.fqdn
output postgreSqlDatabaseName string = postgreSqlServer.outputs.databaseName
output keyVaultName string = keyVault.outputs.name
output keyVaultUri string = keyVault.outputs.uri
output storageAccountName string = storageAccount.outputs.name
output storageAccountBlobEndpoint string = storageAccount.outputs.primaryBlobEndpoint
output mapsAccountName string = mapsAccount.outputs.name
output mapsAccountId string = mapsAccount.outputs.accountId
