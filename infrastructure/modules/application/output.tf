output "api_principal_id" {
    description = "The ID of the app service principal"
    value       = azurerm_linux_web_app.app-nebamgmt-api.identity[0].principal_id
}

output "web_principal_id" {
    description = "The ID of the app service principal"
    value       = azurerm_linux_web_app.app-nebamgmt-web.identity[0].principal_id
}