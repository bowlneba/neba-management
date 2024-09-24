environment = "stage"

resource_group_name = "rg-nebamgmt-stage"
resource_group_budget_threshold = 10

key_vault_name = "kv-nebamgmt-stage"
app_configuration_name = "appcs-nebamgmt-stage"

app_service_plan_name = "asp-nebamgmt-stage"
app_service_plan_sku_name = "SHARED"

api_service_name = "app-nebamgmt-api"
api_always_on = true

web_service_name = "app-nebamgmt-web"
web_always_on = true