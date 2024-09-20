terraform {
  backend "azurerm" {
    resource_group_name  = "infrastructure"
    storage_account_name = "nebamgmtinfrastructure"
    container_name       = "terraform-state"
    key                  = "stage.tfstate"
  }
}