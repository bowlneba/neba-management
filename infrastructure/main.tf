terraform {
  backend "azurerm" {
    resource_group_name = var.backend_resource_group_name
    storage_account_name = var.backend_storage_account_name
    container_name = var.backend_container_name
    key = "${var.environment}.tfstate"
  }
}

provider "azurerm" {
    features {}   
}

module "resource_group" {
  source = "./modules/resource_group"
  resource_group_name = var.resource_group_name
  location  = var.primary_location
  environment = var.environment
  owner = var.owner
}