output "key_vault_id" {
    description = "The ID of the key vault"
    value       = azurerm_key_vault.kv-nebamgmt.id
}

output "key_vault_url" {
    description = "The URL of the key vault"
    value       = azurerm_key_vault.kv-nebamgmt.vault_uri
}