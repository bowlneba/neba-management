// App Service Plan Module
// Equivalent to Terraform's azurerm_app_service_plan resource

@description('Name of the App Service Plan')
param name string

@description('Azure region for the App Service Plan')
param location string

@description('App Service Plan SKU')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1v2'
  'P2v2'
  'P3v2'
])
param sku string

@description('Tags to apply to the resource')
param tags object = {}

// App Service Plan resource
resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
  }
}

// Outputs - These are like Terraform output values from a module
output id string = appServicePlan.id
output name string = appServicePlan.name
