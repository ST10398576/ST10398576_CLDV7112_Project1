# ST10398576_CLDV7112_Project2

ABC Retail's cloud-based order processing platform, built for CLDV7112 (Cloud Development B). The solution has two deployable projects that share one Azure Storage account:

ST10398576_CLDV7112_Project1 — an ASP.NET Core MVC web application for managing customer profiles, products, product images, order/inventory messages and application log files.
ST10398576_ABCRetail.Functions — an Azure Functions app (Project 2) that wraps the storage layer in four HTTP-triggered functions, so the web application talks to Azure Functions instead of the Azure Storage SDK directly.

## Architecture
Browser → MVC Web App (App Service) → Azure Functions App → Azure Storage
                                                                 ├─ Tables  (CustomerProfiles, Products)
                                                                 ├─ Blobs  (productimages)
                                                                 ├─ Queue  (orderprocessing)
                                                                 └─ Files  (logfiles)

The MVC app no longer holds storage account keys for write operations — only the Functions app does. This keeps the write path independently scalable and separates storage credentials from the public-facing web tier.

## Key features
Customer management, backed by Azure Table Storage via the StoreTableEntity function
Product catalogue, backed by the same function against a second table
Image upload, listing and download (Blob Storage) — upload goes through the UploadBlob function
Order/inventory queue messaging (Queue Storage) via WriteToQueue and a queue-triggered ProcessQueueMessage consumer
Log file writing, browsing and download (Azure Files) via UploadToFileShare
Responsive Bootstrap-based UI, consistent card-based theme across views

## Prerequisites
.NET 10 SDK (MVC web project)
.NET 8 SDK, isolated worker model (Functions project)
Visual Studio 2026 (or VS Code + dotnet CLI + Azure Functions Core Tools)
An Azure Storage account with Tables, Blobs, Queue and File Share services enabled
An Azure Functions App and an Azure App Service (Consumption or App Service plan)

## Configuration
### MVC web project — appsettings.json:

json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net"
  },
  "FunctionApi": {
    "BaseUrl": "https://<your-function-app>.azurewebsites.net/api/",
    "Key": "<function app host key>"
  }
}

### Functions project — local.settings.json (git-ignored; never commit this file):

json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net"
  }
}

Use the same storage account connection string in both projects.

## Local development
1. Open the solution in Visual Studio.
2. Set both the web project and the Functions project as startup projects (Solution Properties → Multiple startup projects), or run them in two terminals with dotnet run.
3. Add the connection strings above for local testing (or use dotnet user-secrets for the web project).
4. Confirm the Functions project lists all five functions on startup (StoreTableEntity, UploadBlob, WriteToQueue, ProcessQueueMessage, UploadToFileShare).

## Deployment to Azure
1. Deploy the Functions project first (Visual Studio → Publish → Azure Function App), so you have a live URL and host key.
2. In the Function App's Configuration → Application settings, add StorageConnectionString with the storage account connection string. This does not travel with local.settings.json.
3. If using a Consumption plan, be aware the queue trigger can go cold between invocations; an App Service plan with Always On enabled keeps it responsive.
4. Update the MVC app's appsettings.json with the deployed Function App's URL and host key, then publish the web app to its own App Service.
5. Do not commit local.settings.json or any file containing live storage keys to source control.

## Troubleshooting
"The function call failed" with no further detail — usually the Function App's host key in the MVC app's config is missing, wrong, or stale from a previous deployment. Re-copy it from Function App → App keys, and republish the MVC app after any appsettings.json change.
Function returns 500 — check that StorageConnectionString is set in the Function App's own Application settings (separate from the MVC app's settings).
Table writes succeed but don't appear in the app — Azure Table Storage table names are case-sensitive; make sure the function writes to the exact same table name (case included) that the read code queries.
Queue messages show as garbled text — confirm both the function and the MVC app's queue client use the same message encoding (Base64) consistently.
Image upload always reports "no file selected" — check that the upload form's file input name attribute matches the controller action's IFormFile parameter name exactly.


## License
This project does not include a license file by default. Add a LICENSE file if you intend to publish.