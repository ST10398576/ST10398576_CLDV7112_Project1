# ST10398576_CLDV7112_Project1

ASP.NET web application demo that uses Azure Storage services (Tables, Blobs, Queues, Files). The project includes a Razor Pages / MVC-style UI for managing customers, products, images, queue messages and viewing logs.

Key features
- Customer management backed by Azure Table Storage
- Product list and add (Table Storage)
- Image upload, listing and download (Blob Storage)
- Queue message send/list (Queue Storage)
- Log file browsing and download (File Storage)
- Responsive Bootstrap-based UI (consistent theme across views)

Prerequisites
- .NET 10 SDK
- Visual Studio 2026 (or VS Code + dotnet CLI)
- An Azure Storage account (for full functionality)

Configuration
- Set the Azure storage connection string in appsettings.json OR as an environment variable named `AzureStorage__ConnectionString`.

  Example connection string format:

  ```text
  DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net
  ```

Local development
- Open the solution `ST10398576_CLDV7112_Project1.slnx` in Visual Studio and set the web project as the startup project.
- Add the storage connection string to `appsettings.json` for local testing or use `dotnet user-secrets`.
- Press F5 or run `dotnet run` from the project folder.

UI and theme
- The application uses Bootstrap 5 and a consistent card-based theme. The main layout is in `Views/Shared/_Layout.cshtml` and overrides in `wwwroot/css/site.css`.
- The Customer Index view was used as the visual reference; other views were updated to match its container/card/table styles.

Deployment to Azure
- Create or use an existing Azure Storage account.
- Create an App Service and deploy from Visual Studio (Publish) or use GitHub Actions / Azure Pipelines.
- In App Service > Configuration, add `AzureStorage__ConnectionString` with your storage connection string. Do not commit secrets to source control.

Troubleshooting
- If you see permission errors for blobs, ensure the connection string has the Storage account key (not SAS-only) so server-side SAS generation works.
- If any UI pages look plain, confirm the `_Layout.cshtml` includes the Bootstrap CDN links and `wwwroot/css/site.css` is present.

Contributing
- Feel free to open issues or pull requests on the repository. For UI changes, edit the views under `Views/` and styles in `wwwroot/css/site.css`.

License
- This project does not include a license file by default. Add a LICENSE file if you intend to publish.

Need more?
- I can add CI/CD, ARM/Terraform for infra provisioning, or refine the color palette and typography. Tell me which you'd like.

