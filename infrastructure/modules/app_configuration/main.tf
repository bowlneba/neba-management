data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv-nebamgmt" {
  name = var.key_vault_name
  location = var.location
  resource_group_name = var.resource_group_name
  sku_name = "standard"
  tenant_id = data.azurerm_client_config.current.tenant_id
  enable_rbac_authorization = true

  tags = {
    "environment" = var.environment,
    "owner" = var.owner
  }
}

resource "azurerm_app_configuration" "appcs-nebamgmt" {
  name = var.app_configuration_name
  resource_group_name = var.resource_group_name
  location = var.location

  sku = "free"

  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled = false

  tags = {
    "environment" = var.environment,
    "owner" = var.owner
  }
}

data "azurerm_role_definition" "key_vault_contributor" {
  name = "Key Vault Contributor"
}

resource "azurerm_role_assignment" "infrastructure-key-vault-contributor" {
  scope = azurerm_key_vault.kv-nebamgmt.id
  role_definition_id = data.azurerm_role_definition.key_vault_contributor.name
  principal_id = data.azurerm_client_config.current.object_id
}

data "azurerm_role_definition" "key_vault_secrets_user" {
  name = "Key Vault Secrets User"
}

resource "azurerm_role_assignment" "infrastructure-key-vault-secrets-user" {
  scope = azurerm_key_vault.kv-nebamgmt.id
  role_definition_id = data.azurerm_role_definition.key_vault_secrets_user.name
  principal_id = var.api_principal_id
}

data "azurerm_role_definition" "app_configuration_contributor" {
  name = "App Configuration Contributor"
}

resource "azurerm_role_assignment" "infrastructure-app-config-contributor" {
  scope = azurerm_app_configuration.appcs-nebamgmt.id
  role_definition_id = data.azurerm_role_definition.app_configuration_contributor.name
  principal_id = data.azurerm_client_config.current.object_id
}