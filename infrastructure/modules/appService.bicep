// App Service Module
// Equivalent to Terraform's azurerm_app_service or azurerm_linux_web_app resource

@description('Name of the App Service')
param name string

@description('Azure region for the App Service')
param location string

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@description('App settings (environment variables)')
param appSettings array = []

@description('.NET version')
param dotnetVersion string = '8.0'

@description('Tags to apply to the resource')
param tags object = {}

// App Service resource
resource appService 'Microsoft.Web/sites@2024-11-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|${dotnetVersion}'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: appSettings
    }
    httpsOnly: true
  }
}

// Outputs
output id string = appService.id
output name string = appService.name
output defaultHostName string = appService.properties.defaultHostName
