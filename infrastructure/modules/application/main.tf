resource "azurerm_service_plan" "asp-nebamgmt" {
  name = var.app_service_plan_name
  location = var.location
  resource_group_name = var.resource_group_name
  os_type = "Linux"
  sku_name = var.app_service_plan_sku_name

  tags = {
    "environment" = var.environment,
    "owner" = var.owner
  }
}