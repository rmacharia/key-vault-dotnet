using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add Key Vault URL
var keyVaultUrl = "https://rgvault254.vault.azure.net/";

builder.Services.AddRazorPages();
builder.Services.AddSingleton<SecretClient>(provider => 
    new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));


var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
try
{
    var secret = await secretClient.GetSecretAsync("AppSecret");
    builder.Configuration["AppSecret"] = secret.Value.Value;
}
catch { /* Handle missing secret */ }

var app = builder.Build();

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

// Add debug endpoint to verify secret
app.MapGet("/config", async (SecretClient secretClient) => 
{
    try
    {
        var secret = await secretClient.GetSecretAsync("AppSecret");
        return Results.Ok($"AppSecret value: {secret.Value.Value}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.Run();