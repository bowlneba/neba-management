output "app_configuration_id" {
    description = "The ID of the app configuration"
    value       = azurerm_app_configuration.appcs-nebamgmt.id
  
}

output "app_config_endpoint" {
    description = "The endpoint of the app configuration"
    value       = azurerm_app_configuration.appcs-nebamgmt.endpoint
}

output "key_vault_id" {
    description = "The ID of the key vault"
    value       = azurerm_key_vault.kv-nebamgmt.id
}

output "infrastructure-key-vault-contributor-id" {
    description = "The ID of the infrastructure key vault contributor role"
    value       = azurerm_role_assignment.infrastructure-key-vault-contributor.id
}