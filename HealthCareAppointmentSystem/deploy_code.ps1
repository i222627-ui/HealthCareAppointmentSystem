$ErrorActionPreference = "Stop"

Write-Host "Configuring Azure App Settings..."
az webapp config connection-string set --resource-group HealthCareApp-RG-90180 --name healthcare-app-90180 --settings DefaultConnection="Server=tcp:healthcaresql-90180.database.windows.net,1433;Initial Catalog=HealthCareDB;Persist Security Info=False;User ID=sqladmin;Password=P@ssw0rd1234!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" --connection-string-type SQLServer

Write-Host "Building and Publishing the App..."
cd HealthCareAppointmentSystem
dotnet publish -c Release -o ../publish
cd ..

Write-Host "Zipping the deployment package..."
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

Write-Host "Deploying to Azure Web App..."
az webapp deploy --resource-group HealthCareApp-RG-90180 --name healthcare-app-90180 --src-path ./publish.zip

Write-Host "Deployment Complete! Your site is live at: https://healthcare-app-90180.azurewebsites.net"
