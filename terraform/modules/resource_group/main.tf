variable "resource_group_name" {
  description = "value for the resource group name"
  default     = "nebamgmt-rg-test"
  type        = string
}

variable "location"{
    description = "value for the location"
    default     = "East US"
    type        = string
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.resource_group_name
  location = var.location
}

output "resource_group_name" {
  description = "value for the resource group name"
  value       = azurerm_resource_group.nebamgmt-rg.name
}

output "resource_group_id" {
    description = "value for the resource group id"
    value       = azurerm_resource_group.nebamgmt-rg.id
}

output "resourse_group_location" {
	description = "value for the resourse group location"
	value = azurerm_resourse_group.nebamgmt-rg.location
}