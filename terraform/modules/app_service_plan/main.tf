variable "app_service_plan_name" {
  description = "value for the app service plan name"
  type        = string
}

variable "app_service_plan_location"{
    description = "value for the app service plan location"
    type        = string
}

variable "app_service_plan_resource_group_name"{
    description = "value for the app service plan resource group name"
    type        = string
}

variable "app_service_plan_sku_name" {
  description = "value for the app service plan sku name"
  type        = string
}

resource "azurerm_service_plan" "nebamgmt-asp" {
  name                = var.app_service_plan_name
  location            = var.app_service_plan_location
  resource_group_name = var.app_service_plan_resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku_name
}

output "app_service_plan_id" {
    description = "value for the app service plan id"
    value       = azurerm_service_plan.nebamgmt-asp.id
}