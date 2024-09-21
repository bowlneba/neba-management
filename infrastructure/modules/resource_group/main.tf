resource "azurerm_resource_group" "rg-nebamgmt" {
  name = var.resource_group_name
  location = var.location
  tags = {
    environment = var.environment
    owner = var.owner
  }
}

resource "azurerm_monitor_action_group" "ag-nebamgmt-budget" {
  name = "Budget Action Group"
  resource_group_name = azurerm_resource_group.rg-nebamgmt.name
  short_name = "BudgetAG"

  email_receiver {
    name = "System Admin"
    email_address = var.system_admin_email
  }

  email_receiver {
    name = "Manager"
    email_address = var.manager_email
  }
}