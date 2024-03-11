provider "azurerm" {
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
  internet_ingestion_enabled = true
  internet_query_enabled = true
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
  client_certificate_enabled = false

  site_config {
    always_on = var.api_always_on
    application_stack {
      dotnet_version = "8.0"
    }
    remote_debugging_version = "VS2022"
  }

  auth_settings {
    enabled = false
  }

  https_only = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.nebamgmt-ai.instrumentation_key
    "APPINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.nebamgmt-ai.connection_string
  }

  connection_string {
    name = "AppConfig"
    type = "Custom"
    value = azurerm_app_configuration.nebamgmt-config.primary_read_key[0].connection_string
  }

  identity {
    type = "SystemAssigned"
  }
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
  client_certificate_enabled = false

  site_config {
    always_on = var.api_always_on
    application_stack {
      dotnet_version = "8.0"
    }
    remote_debugging_version = "VS2022"
  }

  auth_settings {
    enabled = false
  }

  depends_on = [azurerm_linux_web_app.nebamgmt-api]

  https_only = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.nebamgmt-ai.instrumentation_key
    "APPINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.nebamgmt-ai.connection_string
  }

  connection_string {
    name = "AppConfig"
    type = "Custom"
    value = azurerm_app_configuration.nebamgmt-config.primary_read_key[0].connection_string
  }

  identity {
    type = "SystemAssigned"
  }
}

variable "nebamgmt_key_vault_name" {
  description = "value for the key vault name"
  type = string
}

resource "azurerm_key_vault" "nebamgmt-kv" {
  name                = var.nebamgmt_key_vault_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  sku_name            = "standard"
  tenant_id           = data.azurerm_client_config.current.tenant_id
  enable_rbac_authorization = true
}

data "azurerm_role_definition" "keyvault_admin" {
  name = "Key Vault Administrator"
}

variable "azure_infrastructure_management_group_id"{
  description = "value for the azure infrastructure management group id"
  default     = "00000000-0000-0000-0000-000000000000"
  type        = string
}

variable "azure_nebamgmt_local_app_registration_object_id"{
  description = "value for the nebamgmt local app registration id"
  default     = "00000000-0000-0000-0000-000000000000"
  type        = string
}

resource "azurerm_role_assignment" "infrastructure_mgmt_kv_admin" {
  scope                = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_admin.name
  principal_id         = var.azure_infrastructure_management_group_id
}

resource "azurerm_role_assignment" "infrastructure_mgmt_kv_user" {
  scope                = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_secrets_user.name
  principal_id         = var.azure_infrastructure_management_group_id
}

resource "azurerm_role_assignment" "nebamgmt-local-kv-user" {
  scope                = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_secrets_user.name
  principal_id         = var.azure_nebamgmt_local_app_registration_object_id
}

variable "nebamgmt_api_url" {
  description = "value for the nebamgmt api url"
  type        = string
}

data "azurerm_role_definition" "keyvault_secrets_user" {
  name = "Key Vault Secrets User"
}

resource "azurerm_role_assignment" "nebamgmt-api-kv-secrets-user"{
  scope = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_secrets_user.name
  principal_id = azurerm_linux_web_app.nebamgmt-api.identity.0.principal_id
}

resource "azurerm_role_assignment" "nebamgmt-ui-kv-secrets-user"{
  scope = azurerm_key_vault.nebamgmt-kv.id
  role_definition_name = data.azurerm_role_definition.keyvault_secrets_user.name
  principal_id = azurerm_linux_web_app.nebamgmt-ui.identity.0.principal_id
}

variable "nebamgmt_config_name" {
  description = "value for the nebamgmt config name"
  type        = string
}

resource "azurerm_app_configuration" "nebamgmt-config"{
  name = var.nebamgmt_config_name
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  location = azurerm_resource_group.nebamgmt-rg.location

  sku = "free"
}

data "azurerm_role_definition" "app_config_data_reader" {
  name = "App Configuration Data Reader"
}

resource "azurerm_role_assignment" "nebamgmt-api-app-config-data-reader"{
  scope = azurerm_app_configuration.nebamgmt-config.id
  role_definition_name = data.azurerm_role_definition.app_config_data_reader.name
  principal_id = azurerm_linux_web_app.nebamgmt-api.identity.0.principal_id
}

resource "azurerm_role_assignment" "nebamgmt-ui-app-config-data-reader"{
  scope = azurerm_app_configuration.nebamgmt-config.id
  role_definition_name = data.azurerm_role_definition.app_config_data_reader.name
  principal_id = azurerm_linux_web_app.nebamgmt-ui.identity.0.principal_id
}

data "azurerm_role_definition" "appconfig_data_owner"{
  name = "App Configuration Data Owner"
}

resource "azurerm_role_assignment" "nebamgmt-infrastructure-mgmt-app-config-admin"{
  scope = azurerm_app_configuration.nebamgmt-config.id
  role_definition_name = data.azurerm_role_definition.appconfig_data_owner.name
  principal_id = var.azure_infrastructure_management_group_id
}

resource "azurerm_app_configuration_key" "nebamgmt-app-config-kv-url-value" {
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  key = "KeyVault--Url"
  value = azurerm_key_vault.nebamgmt-kv.vault_uri
  label = "KeyVaultUrl"

  depends_on = [ azurerm_role_assignment.nebamgmt-infrastructure-mgmt-app-config-admin ]
}

resource "azurerm_app_configuration_key" "nebamgmt-api-url-key"{
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  key = "NebaApi--BaseUrl"
  value = var.nebamgmt_api_url
  label = "ApiUrl"
  
  depends_on = [ azurerm_role_assignment.nebamgmt-infrastructure-mgmt-app-config-admin ]
}

resource "azurerm_app_configuration_feature" "test-feature"{
  name = "Test-Feature"
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  enabled = true

  depends_on = [ azurerm_role_assignment.nebamgmt-infrastructure-mgmt-app-config-admin ]
}