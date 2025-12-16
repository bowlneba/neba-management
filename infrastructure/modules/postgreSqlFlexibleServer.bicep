// PostgreSQL Flexible Server Module
// Equivalent to Terraform's azurerm_postgresql_flexible_server resource

@description('Name of the PostgreSQL Flexible Server')
param name string

@description('Azure region for the PostgreSQL server')
param location string

@description('Administrator username for PostgreSQL')
param administratorLogin string

@description('Administrator password for PostgreSQL')
@secure()
param administratorLoginPassword string

@description('PostgreSQL version')
@allowed(['17', '16', '15', '14', '13'])
param version string = '17'

@description('SKU name for the server')
param skuName string = 'Standard_B1ms'

@description('Storage size in GB')
@minValue(32)
@maxValue(16384)
param storageSizeGB int = 32

@description('Backup retention days')
@minValue(7)
@maxValue(35)
param backupRetentionDays int = 7

@description('Enable high availability')
param highAvailability bool = false

@description('Database name to create')
param databaseName string

@description('Enable Azure AD authentication')
param enableAzureADAuth bool = true

@description('Enable password authentication')
param enablePasswordAuth bool = true

@description('Enable System-Assigned Managed Identity for the PostgreSQL server')
param enableManagedIdentity bool = true

@description('Tags to apply to the resource')
param tags object = {}

// PostgreSQL Flexible Server resource
resource postgreSqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: name
  location: location
  sku: {
    name: skuName
    tier: startsWith(skuName, 'Standard_B') ? 'Burstable' : startsWith(skuName, 'Standard_D') || startsWith(skuName, 'Standard_E') ? 'MemoryOptimized' : 'GeneralPurpose'
  }
  identity: enableManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: version
    storage: {
      storageSizeGB: storageSizeGB
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: highAvailability ? 'ZoneRedundant' : 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: enableAzureADAuth ? 'Enabled' : 'Disabled'
      passwordAuth: enablePasswordAuth ? 'Enabled' : 'Disabled'
    }
  }
}

// Firewall rule to allow Azure services
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Create the database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgreSqlServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Outputs
output id string = postgreSqlServer.id
output name string = postgreSqlServer.name
output fqdn string = postgreSqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
