// Key Vault Secret Module
// Creates or updates a secret in Azure Key Vault

@description('Name of the Key Vault')
param keyVaultName string

@description('Name of the secret')
param secretName string

@description('Value of the secret')
@secure()
param secretValue string

@description('Content type of the secret (optional)')
param contentType string = ''

@description('Tags to apply to the secret')
param tags object = {}

// Reference the existing Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Create or update the secret
resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretName
  tags: tags
  properties: {
    value: secretValue
    contentType: contentType != '' ? contentType : null
  }
}

// Outputs
output secretUri string = secret.properties.secretUri
output secretName string = secret.name
