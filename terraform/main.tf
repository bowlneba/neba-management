# terraform/main.tf
provider "azurerm" {
  features {}
}

variable "storage_account_name" {
  default = "nebamgmttest"
}

variable "container_name" {
  default = "nebamgmt-terraform-state-test"
}

terraform {
  backend "azurerm" {
    resource_group_name  = nebamgmt-rg.name
    storage_account_name = var.storage_account_name
    container_name       = var.container_name
    key                  = "terraform.tfstate"
  }
}

variable "resource_group_name" {
    default = "nebamgmt-rg-test"
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.resource_group_name
  location = "East US"
}

variable "app_service_plan_name" {
    default = "nebamgmt-asp-test"
}

variable "app_service_plan_sku_name" {
    default = "F1"
}

resource "azurerm_service_plan" "nebamgmt-asp" {
  name                = var.app_service_plan_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  os_type = "Linux"
  sku_name = var.app_service_plan_sku_name
}

variable "app_insights_name"{
    default = "nebamgmt-ai-test"
}

resource "azurerm_application_insights" "nebamgmt-ai" {
  name                = var.app_insights_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  application_type    = "web"
}

variable "api_service_name" {
    default = "nebamgmt-api-test"
}

resource "azurerm_linux_web_app" "nebamgmt-api"{
    name = var.api_service_name
    location = azurerm_resource_group.nebamgmt-rg.location
    resource_group_name = azurerm_resource_group.nebamgmt-rg.name
    service_plan_id = azurerm_service_plan.nebamgmt-asp.id

    site_config {
        always_on = false
    }

    https_only = true

    app_settings = {
        "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.nebamgmt-ai.instrumentation_key
    }
}