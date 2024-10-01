resource "azurerm_mssql_server" "sql-nebamgmt-primary" {
    name = var.nebamgmt_mssql_primary_server_name
    resource_group_name = var.resource_group_name
    location = var.primary_location
    version = "12.0"

    administrator_login = "nebamgmtsa"
    administrator_login_password = var.nebamgmt_mssql_admin_password

    tags = {
        environment = var.environment
        owner = var.owner
    }
}