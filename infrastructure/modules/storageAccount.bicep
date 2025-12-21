// Azure Storage Account Module
// Provisions a storage account with blob storage

@description('Name of the Storage Account')
param name string

@description('Azure region for the Storage Account')
param location string

@description('SKU for the Storage Account')
@allowed(['Standard_LRS', 'Standard_GRS', 'Standard_RAGRS', 'Standard_ZRS', 'Premium_LRS', 'Premium_ZRS'])
param skuName string = 'Standard_LRS'

@description('Access tier for blob storage')
@allowed(['Hot', 'Cool'])
param accessTier string = 'Hot'

@description('Enable HTTPS traffic only')
param httpsOnly bool = true

@description('Minimum TLS version')
@allowed(['TLS1_0', 'TLS1_1', 'TLS1_2'])
param minTlsVersion string = 'TLS1_2'

@description('Allow shared key access')
param allowSharedKeyAccess bool = true

@description('Enable blob public access')
param allowBlobPublicAccess bool = false

@description('Tags to apply to the resource')
param tags object = {}

// Storage Account resource
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
  }
  kind: 'StorageV2'
  properties: {
    accessTier: accessTier
    supportsHttpsTrafficOnly: httpsOnly
    minimumTlsVersion: minTlsVersion
    allowSharedKeyAccess: allowSharedKeyAccess
    allowBlobPublicAccess: allowBlobPublicAccess
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    encryption: {
      keySource: 'Microsoft.Storage'
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
        file: {
          enabled: true
          keyType: 'Account'
        }
      }
    }
  }
}

// Blob service (required for containers)
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// Outputs
output id string = storageAccount.id
output name string = storageAccount.name
output primaryEndpoints object = storageAccount.properties.primaryEndpoints
output primaryBlobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
