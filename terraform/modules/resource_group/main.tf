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

output "resource_group_location" {
	description = "value for the resource group location"
	value = azurerm_resource_group.nebamgmt-rg.location
}

# variable "system_admin_email" {
#   description = "value for the system admin email address"
#   default     = "info@bowlneba.com"
#   type        = string
# }

# variable "manager_email" {
#   description = "value for the manager email address"
#   default     = "manager@bowlneba.com"
#   type        = string
# }

# resource "azurerm_monitor_action_group" "nebamgmt-budget-ag" {
#   name                = "Budget Action Group"
#   resource_group_name = azurerm_resource_group.nebamgmt-rg.name
#   short_name          = "BudgetAG"

#   email_receiver {
#     name          = "System Admin"
#     email_address = var.system_admin_email
#   }

#   email_receiver {
#     name          = "Manager"
#     email_address = var.manager_email
#   }
# }

# variable "resource_group_budget_dollars" {
#   description = "value for the resource group budget in dollars"
#   type        = number
# }

# resource "azurerm_consumption_budget_resource_group" "nebamgmt-rg-budget" {
#   name              = "Resource Group Budget"
#   resource_group_id = azurerm_resource_group.nebamgmt-rg.id
#   amount            = var.resource_group_budget_dollars
#   time_grain        = "Monthly"

#   time_period {
#     start_date = "2024-03-01T00:00:00Z"
#     end_date   = "2030-12-31T23:59:59Z"
#   }

#   notification {
#     operator       = "GreaterThan"
#     threshold      = 50
#     contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
#   }

#   notification {
#     operator       = "GreaterThan"
#     threshold      = 90
#     contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
#   }
# }