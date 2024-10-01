output "key_vault_id" {
    description = "The ID of the key vault"
    value       = azurerm_key_vault.kv-nebamgmt.id
}

output "key_vault_url" {
    description = "The URL of the key vault"
    value       = azurerm_key_vault.kv-nebamgmt.vault_uri
}

output "infrastructure_key_vault_secrets_officer_role_id" {
    description = "The ID of the infrastructure key vault secrets_officer role"
    value       = azurerm_role_assignment.infrastructure-key-vault-secrets-officer.id
}