terraform {
  backend "local" {}
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