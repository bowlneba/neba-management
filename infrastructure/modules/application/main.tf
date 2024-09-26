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

resource "azurerm_linux_web_app" "app-nebamgmt-api" {
  name = var.api_service_name
  location = var.location
  resource_group_name = var.resource_group_name
  service_plan_id = azurerm_service_plan.asp-nebamgmt.id
  client_certificate_enabled = false

  site_config {
    always_on = var.api_always_on

    application_stack {
      dotnet_version = var.dotnet_version
    }

    # health_check_path = "/health"
    # health_check_eviction_time_in_min = 5
  }

  auth_settings {
    enabled = false
  }

  https_only = true

  app_settings = {
    #"APPINSIGHTS_CONNECTION_STRING" = ""
    "APPSETTING_ApiKey" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.secret-nebamgmt-api-key.id})"
    "APPSETTING_KeyVaultUrl" = var.key_vault_url
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    "environment" = var.environment,
    "owner" = var.owner
  }
}

resource "azurerm_key_vault_secret" "secret-nebamgmt-api-key" {
  name = "Api-Key"
  value = var.api_key
  key_vault_id = var.key_vault_id
  content_type = "text/plain"

  tags = {
    "environment" = var.environment,
    "owner" = var.owner
  }

  depends_on = [ 
    var.infrastructure_key_vault_secrets_officer_role_id
   ]
}

resource "azurerm_linux_web_app" "app-nebamgmt-web" {
  name = var.web_service_name
  location = var.location
  resource_group_name = var.resource_group_name
  service_plan_id = azurerm_service_plan.asp-nebamgmt.id
  client_certificate_enabled = false

  site_config {
    always_on = var.web_always_on

    application_stack {
      dotnet_version = var.dotnet_version
    }
  }

  auth_settings {
    enabled = false
  }

  https_only = true

  app_settings = {
    #"APPINSIGHTS_CONNECTION_STRING" = ""

    "APPSETTING_KeyVaultUrl" = var.key_vault_url
    "APPSETTING_ApiBaseUrl" = "https://${azurerm_linux_web_app.app-nebamgmt-api.default_hostname}"
    "APPSETTING_ApiKey" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.secret-nebamgmt-api-key.id})"
  }

  identity {
    type = "SystemAssigned"
  }
}