variable "service_name" {
  description = "value for the api service name"
  default     = "nebamgmt-api-test"
  type        = string
}

variable "location" {
    description = "value for the api location"
    type        = string
    
}

variable "resource_group_name"{
    description = "value for the api resource group name"
    type        = string
}

variable "app_service_plan_id" {
    description = "value for the api service plan id"
    type        = string
}

variable "always_on" {
  description = "value for the api always on setting"
  type        = bool
}

variable "dotnet_version" {
  description = "value for the api dotnet version"
  type = string
}

variable "app_insignts_connection_string" {
  description = "value for the api app insights connection string"
  type = string
}

variable "app_config_endpoint" {
  description = "value for the api app config endpoint"
  type = string
}

resource "azurerm_linux_web_app" "nebamgmt-api" {
  name                = var.service_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = var.app_service_plan_id
  client_certificate_enabled = false

  site_config {
    always_on = var.always_on

    application_stack {
      dotnet_version = var.dotnet_version
    }

    health_check_path = "/health"
    health_check_eviction_time_in_min = 5
  }

  auth_settings {
    enabled = false
  }

  https_only = true

  app_settings = {
    "APPINSIGHTS_CONNECTION_STRING" = var.app_insignts_connection_string
    "APPCONFIG_ENDPOINT" = var.app_config_endpoint
  }

  identity {
    type = "SystemAssigned"
  }
}

output "principal_id" {
  description = "value for the api principal id"
  value       = azurerm_linux_web_app.nebamgmt-api.identity[0].principal_id
}

output "default_hostname" {
  description = "value for the api default hostname"
  value       = azurerm_linux_web_app.nebamgmt-api.default_hostname
}