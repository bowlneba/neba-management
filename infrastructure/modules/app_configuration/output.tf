output "app_configuration_id" {
    description = "The ID of the app configuration"
    value       = azurerm_app_configuration.appcs-nebamgmt.id
  
}

output "app_config_endpoint" {
    description = "The endpoint of the app configuration"
    value       = azurerm_app_configuration.appcs-nebamgmt.endpoint
}