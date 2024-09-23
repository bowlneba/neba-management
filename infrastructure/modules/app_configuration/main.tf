data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv-nebamgmt" {
  name = var.key_vault_name
  location = var.location
  resource_group_name = var.resource_group_name
  sku_name = "standard"
  tenant_id = data.azurerm_client_config.current.tenant_id
  enable_rbac_authorization = true
}