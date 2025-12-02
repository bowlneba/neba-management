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

@description('CORS allowed origins')
param corsAllowedOrigins array = []

@description('.NET version')
param dotnetVersion string = '10.0'

@description('Always On setting - keeps app loaded (requires Basic tier or higher)')
param alwaysOn bool = true

@description('Enable detailed error logging')
param detailedErrorLoggingEnabled bool = true

@description('Enable HTTP logging')
param httpLoggingEnabled bool = true

@description('Enable request tracing')
param requestTracingEnabled bool = true

@description('Enable Application Insights')
param enableApplicationInsights bool = false

@description('Application Insights connection string')
param applicationInsightsConnectionString string = ''

@description('Health check path')
param healthCheckPath string = ''

@description('Startup command to run the application')
param startupCommand string = ''

@description('Tags to apply to the resource')
param tags object = {}

@description('Enable system-assigned Managed Identity')
param enableManagedIdentity bool = false

// App Service resource
resource appService 'Microsoft.Web/sites@2024-11-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  identity: enableManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|${dotnetVersion}'
      alwaysOn: alwaysOn
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      detailedErrorLoggingEnabled: detailedErrorLoggingEnabled
      httpLoggingEnabled: httpLoggingEnabled
      requestTracingEnabled: requestTracingEnabled
      healthCheckPath: healthCheckPath != '' ? healthCheckPath : null
      appCommandLine: startupCommand != '' ? startupCommand : null
      appSettings: union(
        appSettings,
        enableApplicationInsights && applicationInsightsConnectionString != '' ? [
          {
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: applicationInsightsConnectionString
          }
          {
            name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
            value: '~3'
          }
        ] : []
      )
      cors: {
        allowedOrigins: corsAllowedOrigins
        supportCredentials: false
      }
      ipSecurityRestrictions: []
      scmIpSecurityRestrictions: []
      scmIpSecurityRestrictionsUseMain: false
    }
  }
}

// Outputs
output id string = appService.id
output name string = appService.name
output defaultHostName string = appService.properties.defaultHostName
output principalId string = enableManagedIdentity ? appService.identity.principalId : ''
