$ErrorActionPreference = "Stop"
$rand = 8111
$rgName = "HealthCareApp-Win-RG-$rand"
$appName = "healthcare-app-win-$rand"

Write-Host "Creating Resource Group..."
az group create --name $rgName --location centralus | Out-Null

Write-Host "Creating Windows App Service Plan..."
az appservice plan create --name "HealthCareApp-Plan-Win-$rand" --resource-group $rgName --sku F1 --location centralus | Out-Null

Write-Host "Creating Windows Web App (without runtime flag to avoid pipe error)..."
az webapp create --resource-group $rgName --plan "HealthCareApp-Plan-Win-$rand" --name $appName | Out-Null

Write-Host "Setting .NET 8 version..."
az webapp config set --resource-group $rgName --name $appName --net-framework-version v8.0 | Out-Null

Write-Host "Configuring Connection String..."
az webapp config connection-string set --resource-group $rgName --name $appName --settings DefaultConnection="Server=tcp:healthcaresql-90180.database.windows.net,1433;Initial Catalog=HealthCareDB;Persist Security Info=False;User ID=sqladmin;Password=P@ssw0rd1234!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" --connection-string-type SQLServer | Out-Null

Write-Host "Deploying ZIP to IIS (Extremely fast on Windows)..."
az webapp deploy --resource-group $rgName --name $appName --src-path ./publish.zip --type zip

Write-Host "Deployment Complete! Your Windows Server is live at: https://$appName.azurewebsites.net"
