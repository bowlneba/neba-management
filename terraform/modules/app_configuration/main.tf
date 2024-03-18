variable "name" {
  description = "value for the nebamgmt config name"
  type        = string
}

variable "resource_group_name" {
  description = "value for the nebamgmt resource group name"
  type        = string
}

variable "location" {
  description = "value for the nebamgmt location"
  type        = string
}

resource "azurerm_app_configuration" "nebamgmt-config"{
  name = var.name
  resource_group_name = var.resource_group_name
  location = var.location

  sku = "free"

  identity {
    type = "SystemAssigned"
  
  }
}

output "endpoint"{
  description = "value for the app config endpoint"
  value       = azurerm_app_configuration.nebamgmt-config.endpoint
}