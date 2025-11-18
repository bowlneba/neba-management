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
    name: azureAppServicePlanName
    location: azureLocation
    sku: azureAppServicePlanSku
    tags: tags
  }
}

// API App Service Module
module apiAppService 'modules/appService.bicep' = {
  scope: rg
  name: 'apiAppService-deployment'
  params: {
    name: azureApiAppServiceName
    location: azureLocation
    appServicePlanId: appServicePlan.outputs.id
    tags: union(tags, { Component: 'API' })
    corsAllowedOrigins: [
      'https://${azureWebAppServiceName}.azurewebsites.net'
    ]
    appSettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: azureEnvironment
      }
      {
        name: 'WEBSITE_RUN_FROM_PACKAGE'
        value: '1'
      }
    ]
  }
}

// Web App Service Module
module webAppService 'modules/appService.bicep' = {
  scope: rg
  name: 'webAppService-deployment'
  params: {
    name: azureWebAppServiceName
    location: azureLocation
    appServicePlanId: appServicePlan.outputs.id
    tags: union(tags, { Component: 'Web' })
    appSettings: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: azureEnvironment
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
output webAppServiceName string = webAppService.outputs.name
output webAppServiceUrl string = 'https://${webAppService.outputs.defaultHostName}'
