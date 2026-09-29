$rand = Get-Random -Minimum 10000 -Maximum 99999
$sqlName = "healthcaresql-" + $rand
$appName = "healthcare-app-" + $rand
$location = "centralus" # Using Central US to avoid East US capacity limits

Write-Host "Creating Resource Group in $location..."
az group create --name HealthCareApp-RG-$rand --location $location

Write-Host "Creating SQL Server $sqlName in $location..."
az sql server create --name $sqlName --resource-group HealthCareApp-RG-$rand --location $location --admin-user sqladmin --admin-password "P@ssw0rd1234!!"

Write-Host "Creating Firewall Rules..."
az sql server firewall-rule create --resource-group HealthCareApp-RG-$rand --server $sqlName --name AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
az sql server firewall-rule create --resource-group HealthCareApp-RG-$rand --server $sqlName --name AllowAll --start-ip-address 0.0.0.0 --end-ip-address 255.255.255.255

Write-Host "Creating SQL Database (Basic Tier, low cost)..."
az sql db create --resource-group HealthCareApp-RG-$rand --server $sqlName --name HealthCareDB --service-objective Basic

Write-Host "Creating App Service Plan (Free F1 Tier)..."
az appservice plan create --name HealthCareApp-Plan-$rand --resource-group HealthCareApp-RG-$rand --sku F1 --is-linux --location $location

Write-Host "Creating Web App $appName..."
az webapp create --resource-group HealthCareApp-RG-$rand --plan HealthCareApp-Plan-$rand --name $appName --runtime 'DOTNETCORE|8.0'

Write-Host "Writing connection details to output..."
Write-Host "SQL_SERVER_NAME=$sqlName"
Write-Host "APP_NAME=$appName"
Write-Host "RESOURCE_GROUP=HealthCareApp-RG-$rand"
Write-Host "Done setting up Azure resources."
