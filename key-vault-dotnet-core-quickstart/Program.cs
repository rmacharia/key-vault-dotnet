using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add Key Vault URL
var keyVaultUrl = "https://rgvault254.vault.azure.net/";

// Add services
builder.Services.AddRazorPages();
builder.Services.AddSingleton<SecretClient>(provider => 
    new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));

// Configure logging
builder.Logging.AddConsole();

var app = builder.Build();

// Pre-load secret during startup (optional)
using (var scope = app.Services.CreateScope())
{
    var secretClient = scope.ServiceProvider.GetRequiredService<SecretClient>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var secret = await secretClient.GetSecretAsync("AppSecret");
        app.Configuration["AppSecret"] = secret.Value.Value;
        logger.LogInformation("Successfully loaded AppSecret from Key Vault");
    }
    catch (Exception ex)
    {
        logger.LogWarning($"Could not load AppSecret from Key Vault: {ex.Message}");
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Add endpoints to test Key Vault functionality
app.MapGet("/", () => "Key Vault Web App is running!");

app.MapGet("/config", async (SecretClient secretClient, ILogger<Program> logger) => 
{
    try
    {
        var secret = await secretClient.GetSecretAsync("AppSecret");
        logger.LogInformation("Successfully retrieved AppSecret");
        return Results.Ok(new { 
            Message = "Secret retrieved successfully", 
            SecretName = "AppSecret",
            SecretValue = secret.Value.Value,
            LastUpdated = secret.Value.Properties.UpdatedOn
        });
    }
    catch (Exception ex)
    {
        logger.LogError($"Error retrieving secret: {ex.Message}");
        return Results.Problem($"Error retrieving secret: {ex.Message}");
    }
});

app.MapPost("/config", async (SecretClient secretClient, ILogger<Program> logger, SecretRequest request) => 
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.SecretValue))
        {
            return Results.BadRequest("Secret value cannot be empty");
        }
        
        await secretClient.SetSecretAsync("AppSecret", request.SecretValue);
        logger.LogInformation("Successfully updated AppSecret");
        return Results.Ok(new { Message = "Secret updated successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError($"Error setting secret: {ex.Message}");
        return Results.Problem($"Error setting secret: {ex.Message}");
    }
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();

// Record for POST request body
public record SecretRequest(string SecretValue);