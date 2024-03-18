variable "name" {
  description = "value for the nebamgmt config name"
  type        = string
}

variable "resource_group_name" {
  description = "value for the nebamgmt resource group name"
  type        = string
}

variable "location" {
  description = "value for the nebamgmt location"
  type        = string
}

resource "azurerm_app_configuration" "nebamgmt-config"{
  name = var.name
  resource_group_name = var.resource_group_name
  location = var.location

  sku = "free"

  identity {
    type = "SystemAssigned"
  
  }
}

output "endpoint"{
  description = "value for the app config endpoint"
  value       = azurerm_app_configuration.nebamgmt-config.endpoint
}

data "azurerm_role_definition" "app_config_data_reader" {
  name = "App Configuration Data Reader"
}

variable "data_reader_principal_ids"{
    description = "value for the data reader principal ids"
    type        = list(string)
}

resource "azurerm_role_assignment" "app_config_data_readers" {
  count = length(var.data_reader_principal_ids)
  scope = azurerm_app_configuration.nebamgmt-config.id
  role_definition_name = data.azurerm_role_definition.app_config_data_reader.name
  principal_id = var.data_reader_principal_ids[count.index]
}

data "azurerm_role_definition" "appconfig_data_owner"{
  name = "App Configuration Data Owner"
}

variable "data_owner_principal_ids"{
    description = "value for the data owner principal ids"
    type        = list(string)
}

resource "azurerm_role_assignment" "app_config_data_owners" {
  count = length(var.data_owner_principal_ids)
  scope = azurerm_app_configuration.nebamgmt-config.id
  role_definition_name = data.azurerm_role_definition.appconfig_data_owner.name
  principal_id = var.data_owner_principal_ids[count.index]
}

variable "config_values" {
  description = "value for the config values"
  type        = map(string)
}

resource "azurerm_app_configuration_key" "nebamgmt-config-values" {
  count = length(keys(var.config_values))
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  key = keys(var.config_values)[count.index]
  value = values(var.config_values)[count.index]
}

variable "secret_values"{
  description = "value for the secret values"
  type        = map(string)
}

resource "azurerm_app_configuration_key" "nebamgmt-config-secrets" {
  count = length(keys(var.secret_values))
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  key = keys(var.secret_values)[count.index]
  type = "vault"
  vault_key_reference = values(var.config_values)[count.index]
}

variable "features" {
  description = "value for the app config features"
  type        = map(bool)
}

resource "azurerm_app_configuration_feature" "nebamgmt-config-features" {
  count = length(keys(var.features))
  name = keys(var.features)[count.index]
  configuration_store_id = azurerm_app_configuration.nebamgmt-config.id
  enabled = keys(var.features)[count.index]
}