using ST10398576_CLDV7112_Project1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IQueueStorageService, QueueStorageService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Health checks (simple)
builder.Services.AddHealthChecks();

var app = builder.Build();

// TEMPORARY DIAGNOSTIC: always show the detailed developer exception page,
// even when deployed, so we can see the real stack trace. 
// IMPORTANT: revert this before final submission — it exposes internal
// details and should never be left on in production.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();