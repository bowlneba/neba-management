variable "primary_location" {
    description = "The primary location of the resource"
    type = string
    default = "East US"
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

variable "resource_group_name" {
    description = "The name of the resource group"
    type = string
}

variable "resource_group_budget_threshold" {
    description = "The budget threshold for the action group"
    type = number
}

# This is set in environment variable in github action
variable "system_admin_email" {
  description = "value for the system admin email address"
  default     = "tech@bowlneba.com"
  type        = string
}

# This is set in environment variable in github action
variable "manager_email" {
  description = "value for the manager email address"
  default     = "manager@bowlneba.com"
  type        = string
}

variable "key_vault_name" {
  description = "The name of the key vault"
  type = string
}

variable "app_configuration_name" {
  description = "The name of the app configuration"
  type = string
}