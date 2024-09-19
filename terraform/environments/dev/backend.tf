terraform {
  backend "azurerm" {
    resource_group_name  = "bowlneba"
    storage_account_name = "bowlneba"
    container_name       = "terraformstate"
    key                  = "dev/nebamgmt/dev.tfstate"
  }
}