variable "name" {
  description = "value for the app service plan name"
  type        = string
}

variable "location"{
    description = "value for the app service plan location"
    type        = string
}

variable "resource_group_name"{
    description = "value for the app service plan resource group name"
    type        = string
}

variable "sku_name" {
  description = "value for the app service plan sku name"
  type        = string
}

resource "azurerm_service_plan" "nebamgmt-asp" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.sku_name
}

output "id" {
    description = "value for the app service plan id"
    value       = azurerm_service_plan.nebamgmt-asp.id
}