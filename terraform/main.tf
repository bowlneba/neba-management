terraform {
  backend "azurerm" {
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

variable "primary_location"{
  description = "value for the primary region"
  default     = "East US"
  type        = string
}

variable "secondary_location"{
  description = "value for the secondary region"
  default     = "East US 2"
  type        = string
}

variable "nebamgmt_resource_group_name" {
  description = "value for the resource group name"
  type        = string
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.nebamgmt_resource_group_name
  location = var.primary_location
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

variable "nebamgmt_config_name" {
  description = "value for the nebamgmt config name"
  type        = string
}

variable "azure_infrastructure_management_group_id"{
  description = "value for the azure infrastructure management group id"
  default     = "00000000-0000-0000-0000-000000000000"
  type        = string
}

module "app_configuration" {
  source = "./modules/app_configuration"

  name = var.nebamgmt_config_name
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  location = azurerm_resource_group.nebamgmt-rg.location
  data_reader_principal_ids = [
    module.api_application.principal_id,
    module.ui_application.principal_id]
  data_owner_principal_ids = [var.azure_infrastructure_management_group_id]
  config_values = {
    "NebaApi:BaseUrl" = module.api_application.default_hostname
    "KeyVault:Url" = module.key_vault.uri
  }
  secret_values = {}
  features = {
    "Caching" = false
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

module "app_service_plan"{
  source = "./modules/app_service_plan"

  name = var.app_service_plan_name
  location = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  sku_name = var.app_service_plan_sku_name
}

variable "log_analytics_workspace_name" {
  description = "value for the log analytics workspace name"
  type        = string
}

variable "log_analytics_workspace_sku" {
  description = "value for the log analytics workspace sku"
  type        = string
}

variable "app_insights_name" {
  description = "value for the application insights name"
  type        = string
}

module "application_insights"{
  source = "./modules/application_insights"

  location = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  log_analytics_workspace_name = var.log_analytics_workspace_name
  log_analytics_workspace_sku = var.log_analytics_workspace_sku
  app_insights_name = var.app_insights_name
}

variable "api_service_name" {
  description = "value for the api service name"
  type        = string
}

variable "api_always_on" {
  description = "value for the api always on setting"
  type        = bool
}

variable "dotnet_version"{
  description = "value for the api dotnet version"
  type        = string
  default = "8.0"
}

module "api_application"{
  source = "./modules/api_application"

  service_name = var.api_service_name
  location = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  app_service_plan_id = module.app_service_plan.id
  always_on = var.api_always_on
  dotnet_version = var.dotnet_version
  app_insignts_connection_string = module.application_insights.connection_string
  app_config_endpoint = module.app_configuration.endpoint
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

module "ui_application"{
  source = "./modules/ui_application"

  service_name = var.ui_service_name
  location = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  app_service_plan_id = module.app_service_plan.id
  always_on = var.ui_always_on
  dotnet_version = var.dotnet_version
  app_insignts_connection_string = module.application_insights.connection_string
  app_config_endpoint = module.app_configuration.endpoint
}

variable "nebamgmt_key_vault_name" {
  description = "value for the key vault name"
  type = string
}

variable "azure_nebamgmt_local_app_registration_principal_id"{
  description = "value for the nebamgmt local app registration id"
  default     = "00000000-0000-0000-0000-000000000000"
  type        = string
}

module "key_vault" {
  source = "./modules/key_vault"

  name = var.nebamgmt_key_vault_name
  location = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  tenant_id = data.azurerm_client_config.current.tenant_id
  key_vault_admin_principal_ids = [
    var.azure_infrastructure_management_group_id,
    var.azure_nebamgmt_local_app_registration_principal_id]
  key_vault_secret_reader_principal_ids = [
    var.azure_infrastructure_management_group_id,
    var.azure_nebamgmt_local_app_registration_principal_id,
    module.api_application.principal_id,
    module.ui_application.principal_id
  ]
  secrets = {
    "Health" = "Check"
  }
}

variable "nebamgmt_mssql_primary_server_name"{
  description = "SQL Server name"
  type = string
}

variable "nebamgmt_mssql_admin_password" {
  description = "Admin password for SQL Server"
  type = string
}

module "mssql" {
  source = "./modules/mssql"

  primary_server_name = var.nebamgmt_mssql_primary_server_name
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  primary_location = azurerm_resource_group.nebamgmt-rg.location
  admin_password = var.nebamgmt_mssql_admin_password
}