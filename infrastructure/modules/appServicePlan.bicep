// App Service Plan Module
// Equivalent to Terraform's azurerm_app_service_plan resource

@description('Name of the App Service Plan')
param name string

@description('Azure region for the App Service Plan')
param location string

@description('App Service Plan SKU. Common values: B1, B2, B3 (Basic), S1, S2, S3 (Standard), P1v3, P2v3, P3v3 (Premium v3)')
param sku string

@description('Tags to apply to the resource')
param tags object = {}

// App Service Plan resource
resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: name
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: sku
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// Outputs - These are like Terraform output values from a module
output id string = appServicePlan.id
output name string = appServicePlan.name
