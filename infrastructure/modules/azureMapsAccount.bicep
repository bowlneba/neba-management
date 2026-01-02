// Azure Maps Account Module
// Provisions an Azure Maps account for map visualization and location services

@description('Name of the Azure Maps Account')
param name string

@description('Azure region for the Azure Maps Account')
param location string

@description('SKU for the Azure Maps Account')
@allowed(['G2', 'S0', 'S1'])
param skuName string = 'G2'

@description('Tags to apply to the resource')
param tags object = {}

// Azure Maps Account resource
resource mapsAccount 'Microsoft.Maps/accounts@2023-06-01' = {
  name: name
  location: location
  sku: {
    name: skuName
  }
  kind: 'Gen2'
  identity: {
    type: 'SystemAssigned'
  }
  tags: tags
  properties: {
    // Keep local auth enabled to support subscription key for local development
    // RBAC will be used in Azure via managed identity
    disableLocalAuth: false
  }
}

// Outputs
output id string = mapsAccount.id
output name string = mapsAccount.name
output accountId string = mapsAccount.properties.uniqueId
