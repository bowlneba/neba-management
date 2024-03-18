variable "name" {
  description = "value for the key vault name"
  type = string
}

variable "location" {
  description = "value for the key vault location"
  type = string
}

variable "resource_group_name" {
  description = "value for the key vault resource group name"
  type = string
}

variable "tenant_id" {
  description = "value for the key vault tenant id"
  type = string
  
}

resource "azurerm_key_vault" "nebamgmt-kv" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_name            = "standard"
  tenant_id           = var.tenant_id
  enable_rbac_authorization = true
}

output "id" {
  value = azurerm_key_vault.nebamgmt-kv.id
}

output "uri" {
  value = azurerm_key_vault.nebamgmt-kv.vault_uri
}

data "azurerm_role_definition" "keyvault_admin" {
  name = "Key Vault Administrator"
}

variable "key_vault_admin_principal_ids" {
  description = "value for the key vault admin principal ids"
  type = list(string)
}

resource "azurerm_role_assignment" "keyvault_admin" {
  count = length(var.key_vault_admin_principal_ids)
  scope                = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_admin.name
  principal_id         = var.key_vault_admin_principal_ids[count.index]
}

data "azurerm_role_definition" "keyvault_secrets_user" {
  name = "Key Vault Secrets User"
}

variable "key_vault_secret_reader_principal_ids" {
  description = "value for the key vault secret reader principal ids"
    type = list(string)
}

resource "azurerm_role_assignment" "keyvault_secrets_user" {
  count = length(var.key_vault_secret_reader_principal_ids)
  scope                = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_secrets_user.name
  principal_id         = var.key_vault_secret_reader_principal_ids[count.index]
}

variable "secrets" {
  description = "value for the key vault secrets"
  type = map(string)
}

resource "azurerm_key_vault_secret" "nebamgmt-kv-secrets" {
  count = length(keys(var.secrets))
  name         = keys(var.secrets)[count.index]
  value        = values(var.secrets)[count.index]
  key_vault_id = azurerm_key_vault.nebamgmt-kv.id
}

output "health_check_mssql_connection_string_secret_id" {
  description = "value for the health check secret id"
  value = [for s in azurerm_key_vault_secret.nebamgmt-kv-secrets : s.id if s.name == "HealthCheck-MSSQL-ConnectionString"][0]
  sensitive = false
}