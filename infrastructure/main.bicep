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

@description('GitHub Actions service principal object ID for Key Vault access')
param githubServicePrincipalObjectId string

@description('Tags to apply to all resources')
param tags object = {
  Environment: azureEnvironment
  ManagedBy: 'Bicep'
  Project: 'NebaManagement'
}

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
    enableManagedIdentity: true
    tags: union(tags, { Component: 'API' })
    corsAllowedOrigins: [
      'https://${azureWebAppServiceName}-${azureLocation}.azurewebsites.net'
    ]
    startupCommand: 'dotnet Neba.Api.dll'
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
        name: 'WEBSITE_RUN_FROM_PACKAGE'
        value: '1'
      }
      {
        name: 'KeyVaultUri'
        value: keyVault.outputs.uri
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

// RBAC Role Assignment: GitHub Actions Service Principal -> Key Vault Secrets Officer
// Key Vault Secrets Officer role ID: b86a8fe4-44ce-4948-aee5-eccb2c155cd7
module githubKeyVaultAccess 'modules/keyVaultRoleAssignment.bicep' = {
  scope: rg
  name: 'githubKeyVaultAccess-deployment'
  params: {
    keyVaultName: keyVault.outputs.name
    principalId: githubServicePrincipalObjectId
    roleDefinitionId: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
  }
}

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
        name: 'WEBSITE_RUN_FROM_PACKAGE'
        value: '1'
      }
      {
        name: 'NebaApi__BaseUrl'
        value: 'https://${apiAppService.outputs.defaultHostName}'
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
output postgreSqlServerName string = postgreSqlServer.outputs.name
output postgreSqlServerFqdn string = postgreSqlServer.outputs.fqdn
output postgreSqlDatabaseName string = postgreSqlServer.outputs.databaseName
output keyVaultName string = keyVault.outputs.name
output keyVaultUri string = keyVault.outputs.uri
