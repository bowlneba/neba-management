# terraform/main.tf

variable "subscription_id" {
  description = "The subscription ID for the Azure provider"
  type        = string
}

variable "client_id" {
  description = "The client ID for the Azure provider"
  type        = string
}

variable "client_secret" {
  description = "The client secret for the Azure provider"
  type        = string
}

variable "tenant_id" {
  description = "The tenant ID for the Azure provider"
  type        = string
}
provider "azurerm" {
  subscription_id = "${var.subscription_id}"
  client_id       = "${var.client_id}"
  client_secret   = "${var.client_secret}"
  tenant_id       = "${var.tenant_id}"
  features {}
}

terraform {
  backend "azurerm" {
  }
}

data "azurerm_client_config" "current" {}

variable "resource_group_name" {
  description = "value for the resource group name"
  default     = "nebamgmt-rg-test"
  type        = string
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.resource_group_name
  location = "East US"
}

variable "system_admin_email" {
  description = "value for the system admin email address"
  default     = "info@bowlneba.com"
  type        = string
}

variable "manager_email" {
  description = "value for the manager email address"
  default     = "manager@bowlneba.com"
  type        = string
}

resource "azurerm_monitor_action_group" "nebamgmt-budget-ag" {
  name                = "Budget Action Group"
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  short_name          = "BudgetAG"

  email_receiver {
    name          = "System Admin"
    email_address = var.system_admin_email
  }

  email_receiver {
    name          = "Manager"
    email_address = var.manager_email
  }
}

variable "resource_group_budget_dollars" {
  description = "value for the resource group budget in dollars"
  type        = number
}

resource "azurerm_consumption_budget_resource_group" "nebamgmt-rg-budget" {
  name              = "Resource Group Budget"
  resource_group_id = azurerm_resource_group.nebamgmt-rg.id
  amount            = var.resource_group_budget_dollars
  time_grain        = "Monthly"

  time_period {
    start_date = "2024-03-01T00:00:00Z"
    end_date   = "2030-12-31T23:59:59Z"
  }
  notification {
    operator       = "GreaterThan"
    threshold      = 50
    contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
  }

  notification {
    operator       = "GreaterThan"
    threshold      = 90
    contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
  }
}

variable "app_service_plan_name" {
  description = "value for the app service plan name"
  type        = string
}

variable "app_service_plan_sku_name" {
  description = "value for the app service plan sku name"
  type        = string
}

resource "azurerm_service_plan" "nebamgmt-asp" {
  name                = var.app_service_plan_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku_name
}

variable "log_analytics_workspace_name" {
  description = "value for the log analytics workspace name"
  type        = string
}

variable "log_analytics_workspace_sku" {
  description = "value for the log analytics workspace sku"
  type        = string
}

resource "azurerm_log_analytics_workspace" "nebamgmt-log-analytics" {
  name                = var.log_analytics_workspace_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  sku                 = var.log_analytics_workspace_sku
}

variable "app_insights_name" {
  description = "value for the application insights name"
  type        = string
}

resource "azurerm_application_insights" "nebamgmt-ai" {
  name                = var.app_insights_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.nebamgmt-log-analytics.id
}

variable "api_service_name" {
  description = "value for the api service name"
  default     = "nebamgmt-api-test"
  type        = string
}

variable "api_always_on" {
  description = "value for the api always on setting"
  type        = bool
}

resource "azurerm_linux_web_app" "nebamgmt-api" {
  name                = var.api_service_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  service_plan_id     = azurerm_service_plan.nebamgmt-asp.id

  site_config {
    always_on = var.api_always_on
  }

  https_only = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.nebamgmt-ai.instrumentation_key
  }

  identity {
    type = "SystemAssigned"
  }
}

variable "application_name"{
    description = "value for the application name"
    type = string
}

resource "azuread_application" "nebamgmt-app" {
  display_name = var.application_name
}

variable "ui_service_name" {
  description = "value for the ui service name"
  default     = "nebamgmt-ui-test"
  type        = string
}

variable "ui_always_on" {
  description = "value for the ui always on setting"
  type        = bool
}

resource "azurerm_linux_web_app" "nebamgmt-ui" {
  name                = var.ui_service_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  service_plan_id     = azurerm_service_plan.nebamgmt-asp.id

  site_config {
    always_on = var.ui_always_on
  }

  depends_on = [azurerm_linux_web_app.nebamgmt-api]

  https_only = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.nebamgmt-ai.instrumentation_key
  }

  identity {
    type = "SystemAssigned"
  }
}

variable "nebamgmt-key-vault-name" {
  description = "value for the key vault name"
  type = string
}

resource "azurerm_key_vault" "nebamgmt-kv" {
  name                = var.nebamgmt-key-vault-name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  sku_name            = "standard"
  tenant_id           = data.azurerm_client_config.current.tenant_id
}

variable "nebamgmt-api-url" {
  description = "value for the nebamgmt api url"
  type        = string
}

resource "azurerm_key_vault_secret" "nebamgmt-api-url-secret"{
  name         = "NebaApi--BaseUrl"
  value        = var.nebamgmt-api-url
  key_vault_id = azurerm_key_vault.nebamgmt-kv.id
  content_type = "text/url"
}

resource "azurerm_key_vault_access_policy" "nebamgmt-kv-ap-api"{
  key_vault_id = azurerm_key_vault.nebamgmt-kv.id

  tenant_id = data.azurerm_client_config.current.tenant_id
  object_id = azurerm_linux_web_app.nebamgmt-api.identity.0.principal_id

  depends_on = [ azurerm_linux_web_app.nebamgmt-api ]

  secret_permissions = [
    "Get",
    "List"
  ]

  key_permissions = [
    "Get",
    "List"
  ]
}

resource "azurerm_key_vault_access_policy" "nebamgmt-kv-ap-ui"{
  key_vault_id = azurerm_key_vault.nebamgmt-kv.id

  tenant_id = data.azurerm_client_config.current.tenant_id
  object_id = azurerm_linux_web_app.nebamgmt-ui.identity.0.principal_id

  depends_on = [ azurerm_linux_web_app.nebamgmt-ui ]

  secret_permissions = [
    "Get",
    "List"
  ]

  key_permissions = [
    "Get",
    "List"
  ]
}

resource "azurerm_role_assignment" "nebamgmt-app-kv-role"{
    scope = azurerm_key_vault.nebamgmt-kv.id
    principal_id = azuread_application.nebamgmt-app.object_id
    role_definition_name = "Key Vault Contributor"
}