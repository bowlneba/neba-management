variable "primary_server_name"{
  description = "SQL Server name"
  type = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type = string
}

variable "primary_location"{
  description = "SQL Server location"
  type = string
}

variable "admin_password" {
  description = "Admin password for SQL Server"
  type = string
}

resource "azurerm_mssql_server" "nebamgmt-primary-sql-server" {
  name = var.primary_server_name
  resource_group_name = var.resource_group_name
  location = var.primary_location
  version = "12.0"

  administrator_login = "nebamgmtsa"
  administrator_login_password = var.admin_password
}