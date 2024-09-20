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

variable "backend_resource_group_name" {
  description = "The name of the resource group for the backend"
  type        = string
}

variable "backend_storage_account_name" {
  description = "The name of the storage account for the backend"
  type        = string
}

variable "backend_container_name" {
  description = "The name of the container for the backend"
  type        = string
}

variable "resource_group_name" {
    description = "The name of the resource group"
    type = string
}