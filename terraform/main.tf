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
    default = "nebamgmt-rg-test"
}

resource "azurerm_resource_group" "nebamgmt-rg" {
  name     = var.resource_group_name
  location = "East US"
}

resource "azurerm_monitor_action_group" "nebamgmt-budget-ag"{
  name = "Budget Action Group"
  resource_group_name = azurerm_resource_group.nebamgmt-rg.name
  short_name = "BudgetAG"
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
    start_date = "2024-03-01T00:00:00Z"
    end_date = "2030-12-31T23:59:59Z"
  }
  notification{
    operator = "GreaterThan"
    threshold = 50
    contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
  }

  notification{
    operator = "GreaterThan"
    threshold = 90
    contact_groups = [azurerm_monitor_action_group.nebamgmt-budget-ag.id]
  }
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