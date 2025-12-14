// Azure Key Vault Module
// Equivalent to Terraform's azurerm_key_vault resource

@description('Name of the Key Vault')
param name string

@description('Azure region for the Key Vault')
param location string

@description('SKU name for the Key Vault')
@allowed(['standard', 'premium'])
param skuName string = 'standard'

@description('Enable soft delete with 90 days retention')
param enableSoftDelete bool = true

@description('Enable purge protection')
param enablePurgeProtection bool = true

@description('Enable RBAC authorization (recommended over access policies)')
param enableRbacAuthorization bool = true

@description('Allow Azure services to bypass network rules')
param enableAzureServicesAccess bool = true

@description('Tags to apply to the resource')
param tags object = {}

// Key Vault resource
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: skuName
    }
    tenantId: subscription().tenantId
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: 90
    enablePurgeProtection: enablePurgeProtection ? true : null
    enableRbacAuthorization: enableRbacAuthorization
    networkAcls: {
      bypass: enableAzureServicesAccess ? 'AzureServices' : 'None'
      defaultAction: 'Allow'
    }
  }
}

// Outputs
output id string = keyVault.id
output name string = keyVault.name
output uri string = keyVault.properties.vaultUri
