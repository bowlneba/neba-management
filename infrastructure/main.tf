terraform {
  
  backend "azurerm" {
    resource_group_name = ""
    storage_account_name = ""
    container_name = ""
    key = ""
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_deleted_keys_on_destroy = false
      recover_soft_deleted_keys = true
    }
  }
}

module "resource_group" {
  source = "./modules/resource_group"
  location  = var.primary_location
  environment = var.environment
  owner = var.owner

  resource_group_name = var.resource_group_name
  budget_threshold = var.resource_group_budget_threshold
  system_admin_email = var.system_admin_email
  manager_email = var.manager_email
}

module "app_configuration" {
  source = "./modules/app_configuration"
  resource_group_name = module.resource_group.resource_group_name
  location = var.primary_location
  environment = var.environment
  owner = var.owner

  key_vault_name = var.key_vault_name
  app_configuration_name = var.app_configuration_name
}

module "application" {
  source = "./modules/application"
  resource_group_name = module.resource_group.resource_group_name
  location = var.primary_location
  environment = var.environment
  owner = var.owner

  app_service_plan_name = var.app_service_plan_name
  app_service_plan_sku_name = var.app_service_plan_sku_name
}