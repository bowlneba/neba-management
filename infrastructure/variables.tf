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