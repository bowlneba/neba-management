// Key Vault RBAC Role Assignment Module
// Assigns a role to a principal (user, group, or service principal) on a Key Vault

@description('Name of the Key Vault')
param keyVaultName string

@description('Principal ID (object ID) to assign the role to')
param principalId string

@description('Role Definition ID (e.g., Key Vault Secrets User: 4633458b-17de-408a-b874-0445c86b69e6)')
param roleDefinitionId string

@description('Principal type')
@allowed(['User', 'Group', 'ServicePrincipal'])
param principalType string = 'ServicePrincipal'

// Reference the existing Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Create the role assignment
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, principalId, roleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: principalId
    principalType: principalType
  }
}

// Outputs
output roleAssignmentId string = roleAssignment.id
