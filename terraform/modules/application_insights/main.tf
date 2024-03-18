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

variable "app_insights_name" {
  description = "value for the application insights name"
  type        = string
}

resource "azurerm_application_insights" "nebamgmt-ai" {
  name                = var.app_insights_name
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.nebamgmt-log-analytics.id
  internet_ingestion_enabled = true
  internet_query_enabled = true
}

output "app_insights_id" {
  description = "value for the application insights id"
  value       = azurerm_application_insights.nebamgmt-ai.id
}