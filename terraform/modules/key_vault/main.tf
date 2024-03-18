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