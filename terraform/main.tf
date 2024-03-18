terraform {
  backend "azurerm" {
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

variable "primary_region"{
  description = "value for the primary region"
  default     = "East US"
  type        = string
}

variable "secondary_region"{
  description = "value for the secondary region"
  default     = "East US 2"
  type        = string
}

variable "nebamgmt_resource_group_name" {
  description = "value for the resource group name"
  type        = string
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

variable "resource_group_budget_dollars" {
  description = "value for the resource group budget in dollars"
  type        = number
}

module "resource_group"{
  source = "./modules/resource_group"
  name = var.nebamgmt_resource_group_name
  location = var.primary_region
  system_admin_email = var.system_admin_email
  manager_email = var.manager_email
  budget_dollars = var.resource_group_budget_dollars
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

  depends_on = [ module.resource_group, module.key_vault ]

  name = var.nebamgmt_config_name
  resource_group_name = module.resource_group.name
  location = module.resource_group.location
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

  depends_on = [ module.resource_group ]

  name = var.app_service_plan_name
  location = module.resource_group.location
  resource_group_name = module.resource_group.name
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

  depends_on = [ module.resource_group ]

  location = module.resource_group.location
  resource_group_name = module.resource_group.name
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

  depends_on = [ module.resource_group, module.app_service_plan, module.application_insights, module.app_configuration]

  service_name = var.api_service_name
  location = module.resource_group.location
  resource_group_name = module.resource_group.name
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

  depends_on = [ module.resource_group, module.app_service_plan, module.application_insights, module.app_configuration]

  service_name = var.ui_service_name
  location = module.resource_group.location
  resource_group_name = module.resource_group.name
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

  depends_on = [ module.resource_group ]

  name = var.nebamgmt_key_vault_name
  location = module.resource_group.location
  resource_group_name = module.resource_group.name
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

  depends_on = [ module.resource_group ]
  
  primary_server_name = var.nebamgmt_mssql_primary_server_name
  resource_group_name = module.resource_group.name
  primary_location = module.resource_group.location
  admin_password = var.nebamgmt_mssql_admin_password
}