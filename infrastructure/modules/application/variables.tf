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
  default = "8.0"
}

variable "api_service_name" {
  description = "The name of the API service"
  type = string
}

variable "api_always_on" {
  description = "The always on setting for the API service"
  type = bool
}

variable "web_service_name" {
  description = "The name of the web service"
  type = string
}

variable "web_always_on" {
  description = "The always on setting for the web service"
  type = bool
}

variable "azure_github_action_managed_identity_client_id" {
  description = "The client ID of the managed identity for the GitHub action"
  type = string
}

variable "api_key" {
  description = "The API key for the API service"
  type = string
}

variable "key_vault_id" {
  description = "The ID of the key vault"
  type = string
}

variable "key_vault_url" {
  description = "The URL of the key vault"
  type = string
}

variable "infrastructure_key_vault_secrets_officer_role_id" {
  description = "The ID of the infrastructure key vault secrets officer role"
  type = string
}