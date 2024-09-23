variable "key_vault_name" {
  description = "The name of the key vault"
  type = string
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type = string
}

variable "location" {
  description = "value of the primary location"
  type = string
}

variable "environment" {
  description = "The environment to deploy (stage, prod)"
  type = string
}

variable "owner" {
  description = "The owner of the resource"
  type = string
}

variable "app_configuration_name" {
  description = "The name of the app configuration"
  type = string
}

variable "api_principal_id" {
  description = "The principal id of the API"
  type = string
}