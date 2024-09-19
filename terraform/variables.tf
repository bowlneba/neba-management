variable "primary_location" {
    description = "The primary location of the resource"
    type = string
    default = "East US"
}

variable "resource_group_name" {
    description = "The name of the resource group"
    type = string
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