// Azure Maps RBAC Role Assignment Module
// Assigns a role to a principal on an Azure Maps Account

@description('Name of the Azure Maps Account')
param mapsAccountName string

@description('Principal ID (object ID) to assign the role to')
param principalId string

@description('Role Definition ID (Azure Maps Data Reader: 423170ca-a8f6-4b0f-8487-9e4eb8f49baa)')
param roleDefinitionId string

@description('Principal type')
@allowed(['User', 'Group', 'ServicePrincipal'])
param principalType string = 'ServicePrincipal'

// Reference the existing Azure Maps Account
resource mapsAccount 'Microsoft.Maps/accounts@2023-06-01' existing = {
  name: mapsAccountName
}

// Create the role assignment
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: mapsAccount
  name: guid(mapsAccount.id, principalId, roleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: principalId
    principalType: principalType
  }
}

// Outputs
output roleAssignmentId string = roleAssignment.id
