variable "log_analytics_workspace_name" {
  description = "value for the log analytics workspace name"
  type        = string
}

variable "location"{
    description = "value for the log analytics location"
    type        = string
}

variable "resource_group_name"{
    description = "value for the log analytics resource group name"
    type        = string
}

variable "log_analytics_workspace_sku" {
  description = "value for the log analytics workspace sku"
  type        = string
}

resource "azurerm_log_analytics_workspace" "nebamgmt-log-analytics" {
  name                = var.log_analytics_workspace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.log_analytics_workspace_sku
}