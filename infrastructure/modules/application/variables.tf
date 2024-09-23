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

variable "app_service_plan_name" {
  description = "The name of the app service plan"
  type = string
}

variable "app_service_plan_sku_name" {
  description = "The SKU of the app service plan"
  type = string
}

variable "dotnet_version" {
  description = "The version of .NET to use"
  type = string
  default = "9.0"
}

variable "api_service_name" {
  description = "The name of the API service"
  type = string
}

variable "api_always_on" {
  description = "The always on setting for the API service"
  type = bool
}

variable "infrastructure-key-vault-contributor-id" {
  description = "The ID of the contributor role for the key vault"
  type = string
}

variable "api_key" {
  description = "The API key for the API service"
  type = string
}

variable "app_config_id" {
  description = "The ID of the app configuration"
  type = string
}

variable "app_config_endpoint" {
  description = "The endpoint of the app configuration"
  type = string
}

variable "key_vault_id" {
  description = "The ID of the key vault"
  type = string
}