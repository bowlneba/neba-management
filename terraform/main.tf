# terraform/main.tf
provider "azurerm" {
  features {}
}

terraform {
  backend "azurerm" {
    key = "terraform.tfstate"
  }
}

variable "resource_group_name" {
    default = "nebamgmt-rg-dev"
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.resource_group_name
  location = "East US"
}

variable "resource_group_budget_cents" {
    default = 1000
}

variable "resource_group_budget_email" {
    default = "info@bowlneba.com"
}

resource "azurerm_consumption_budget_resource_group" "nebamgmt-rg-budget" {
  name = "Resource Group Budget"
  resource_group_id = azurerm_resource_group.nebamgmt-rg.id
  amount = var.resource_group_budget_cents
  time_grain = "Monthly"

  time_period {
    start_date = "2023-03-01"
    end_date = "2030-12-31"
  }

  notification{
    operator = "GreaterThan"
    threshold = 50
    contact_emails = [var.resource_group_budget_email]
  }

  notification{
    operator = "GreaterThan"
    threshold = 90
    contact_emails = [var.resource_group_budget_email]
  }
}

variable "app_service_plan_name" {
    default = "nebamgmt-asp-dev"
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
    default = "nebamgmt-ai-dev"
}

resource "azurerm_application_insights" "nebamgmt-ai" {
  name                = var.app_insights_name
  location            = azurerm_resource_group.nebamgmt-rg.location
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  application_type    = "web"
}

variable "api_service_name" {
    default = "nebamgmt-api-dev"
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