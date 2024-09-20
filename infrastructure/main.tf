provider "azurerm" {
    subscription_id = var.azure_subscription_id
    features {}   
}

module "resource_group" {
  source = "./modules/resource_group"
  resource_group_name = var.resource_group_name
  location  = var.primary_location
  environment = var.environment
  owner = var.owner
}