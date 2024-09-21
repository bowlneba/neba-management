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

variable "system_admin_email" {
  description = "value for the system admin email address"
  default     = "tech@bowlneba.com"
  type        = string
}

variable "manager_email" {
  description = "value for the manager email address"
  default     = "manager@bowlneba.com"
  type        = string
}

variable "budget_threshold" {
  description = "The budget threshold for the action group"
  type        = number
}