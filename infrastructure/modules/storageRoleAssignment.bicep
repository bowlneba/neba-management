// Storage Account RBAC Role Assignment Module
// Assigns a role to a principal on a Storage Account

@description('Name of the Storage Account')
param storageAccountName string

@description('Principal ID (object ID) to assign the role to')
param principalId string

@description('Role Definition ID (e.g., Storage Blob Data Contributor: ba92f5b4-2d11-453d-a403-e96b0029c9fe)')
param roleDefinitionId string

@description('Principal type')
@allowed(['User', 'Group', 'ServicePrincipal'])
param principalType string = 'ServicePrincipal'

// Reference the existing Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

// Create the role assignment
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, principalId, roleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: principalId
    principalType: principalType
  }
}

// Outputs
output roleAssignmentId string = roleAssignment.id
