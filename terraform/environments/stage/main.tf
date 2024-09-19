terraform {
  backend "azurerm" {
    resource_group_name  = "bowlneba"
    storage_account_name = "nebainfrastructure"
    container_name       = "stage"
    key                  = "nebamgmt.tfstate"
  }
}