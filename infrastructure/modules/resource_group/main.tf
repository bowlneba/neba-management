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

resource "azurerm_consumption_budget_resource_group" "budget-nebamgmt" {
  name = "Resource Group Budget"
  resource_group_id = azurerm_resource_group.rg-nebamgmt.id
  amount = var.budget_threshold
  time_grain = "Monthly"

  time_period {
    start_date = "2025-01-01T00:00:00Z"
    end_date = "2099-12-31T23:59:59Z"
  }

  notification {
    operator = "GreaterThan"
    threshold = 50
    contact_groups = [azurerm_monitor_action_group.ag-nebamgmt-budget.id]
  }

  notification {
    operator = "GreaterThan"
    threshold = 90
    contact_groups = [azurerm_monitor_action_group.ag-nebamgmt-budget.id]
  }
}