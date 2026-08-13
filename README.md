# ST10398576_CLDV7112_Project1

This application demonstrates use of Azure Storage services: Tables, Blobs, Queues, and Files.

Required configuration

- Set the Azure storage connection string in appsettings.json or (preferably) as an environment variable named `AzureStorage__ConnectionString` in Azure App Service configuration. Example:

  ```text
  DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net
  ```

Running locally

- In Visual Studio, set the project as startup and press F5.
- Ensure appsettings.json contains the storage connection string for local testing (or use `dotnet user-secrets`).

Deployment to Azure

- Create an Azure Storage account (if you don't have one).
- Create an App Service and configure `AzureStorage__ConnectionString` in Configuration -> Application settings.
- Deploy the project via Publish from Visual Studio.
- Use the /health endpoint to verify the app is running.

Notes

- Do not commit secrets to source control. Use user-secrets for local development and App Service settings or Key Vault for production.
- The app provides UI to manage customers (Tables), upload images (Blobs), queue messages (Queues), and view logs (Files).

If you want, I can add CI/CD or an ARM/Terraform script to provision the storage account and App Service.
