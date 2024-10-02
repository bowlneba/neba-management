variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "primary_location" {
  description = "Primary location"
  type        = string
}

variable "environment" {
    description = "The environment to deploy (stage, prod)"
    type = string
}

variable "owner" {
    description = "The owner of the resource"
    type = string
    default = "David Kipperman"
}

variable "mssql_primary_server_name" {
  description = "Primary SQL Server name"
  type        = string
}

variable "mssql_admin_password" {
  description = "SQL Server admin password"
  type        = string
}