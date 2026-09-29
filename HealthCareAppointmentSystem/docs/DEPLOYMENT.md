# Deployment Strategy & Architecture

This document outlines the cloud deployment strategy for the HealthCare Appointment System, detailing the infrastructure, services, and tooling used to host the application.

## 1. Cloud Provider: Microsoft Azure
Microsoft Azure was chosen as the cloud provider to host both the relational database and the .NET 8 Web Application. Given that the project is built using C# and ASP.NET Core, Azure provides first-class support and native integrations for the ecosystem.

We are utilizing **Azure Free Tier (F1)** to keep hosting costs at zero while maintaining full functionality.

### Core Azure Services:
1. **Azure SQL Database (Basic Tier):**
   - **Why:** Replaces the local SQL Server Express used during development. It securely hosts the relational schema (Patients, Doctors, Appointments, Invoices, Lab Orders, etc.).
   - **How it works:** We use Entity Framework Core (`dotnet ef database update`) pointed at the Azure SQL Connection String to automatically generate the database schema directly in the cloud.

2. **Azure App Service (Windows F1 Tier):**
   - **Why:** Hosts the actual .NET 8 web application. We explicitly chose a **Windows** App Service Plan instead of Linux. While Azure defaults to Linux for Free tiers in some regions, Linux Free tiers rely on Kudu Docker containers that frequently timeout or crash (Out of Memory) when unpacking large deployment bundles (e.g., our 96MB `publish.zip`). Windows App Services run natively on IIS and can mount and extract deployment payloads instantly without container overhead.

3. **Azure Resource Groups:**
   - **Why:** Used to logically group our App Service and SQL Database. If we ever need to tear down the environment, deleting the Resource Group deletes everything cleanly.

---

## 2. Deployment Tooling & Scripts

To automate the provisioning of cloud resources, we created several PowerShell scripts utilizing the **Azure CLI (`az`)**. These scripts ensure infrastructure as code (IaC) principles.

### Key Scripts:
- `deploy_azure.ps1`: Initial script that provisions the Resource Group, SQL Server, SQL Database, Firewall rules, and App Service Plan.
- `deploy_code.ps1`: Automates the `dotnet publish` local build process, compresses the output into `publish.zip`, and pushes it to Azure via `az webapp deploy`.
- `deploy_windows.ps1`: A specialized remediation script designed to delete stalled Linux App Services and forcefully provision a native Windows App Service environment to bypass Free Tier Kudu memory limitations.

---

## 3. The CI/CD Pipeline (Next Steps)

While the PowerShell scripts (`deploy_*.ps1`) are excellent for initial provisioning and emergency direct-uploads (`ZIP Deploy`), manual ZIP deployments are not best practice for production applications.

### GitHub Actions Integration
Moving forward, the application is designed to be deployed using **Continuous Integration and Continuous Deployment (CI/CD)** via **GitHub Actions**.

**How it will work:**
1. The Azure App Service is linked to the `main` branch of the GitHub repository via the Azure Portal (Deployment Center).
2. When a developer pushes code to GitHub (`git push origin main`), a GitHub Action is automatically triggered.
3. GitHub's enterprise runners (which have significantly more RAM and CPU than the Azure Free Tier) will execute `dotnet build` and `dotnet publish`.
4. GitHub automatically pushes the compiled, production-ready artifact directly into the Azure App Service.

**Benefits:**
- Eliminates local dependency on deployment scripts.
- Prevents timeout issues caused by uploading large ZIP files from local machines.
- Ensures the cloud environment is always perfectly synced with the `main` GitHub branch.

---

## 4. Known Limitations & Workarounds
- **Azure F1 Throttling:** Microsoft strictly limits the rate at which Free Tier resources can be deleted and recreated. If an App Service is recreated too rapidly, the subscription will receive a `Throttled` error blocking new creations for a few hours.
- **Workaround:** If the frontend Web App is temporarily blocked, developers can update their local `appsettings.json` to point `DefaultConnection` to the live Azure SQL Database. This allows local development (`localhost:5000`) to continue utilizing the live cloud backend seamlessly until the frontend throttling lifts.
