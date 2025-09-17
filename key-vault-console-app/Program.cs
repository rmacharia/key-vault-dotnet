using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace KeyVaultConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string secretName = "AppSecret";
            
            // Authentication
            string keyVaultName = "rgvault254";
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";
            
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            
            Console.Write("Input the value of your secret > ");
            string secretValue = Console.ReadLine();
            
            Console.Write($"Creating a secret in {keyVaultName} called '{secretName}' with the value '{secretValue}' ...");
            
            try
            {
                // Set secret
                await client.SetSecretAsync(secretName, secretValue);
                Console.WriteLine(" done.");
                
                Console.WriteLine("Forgetting your secret.");
                secretValue = "";
                Console.WriteLine($"Your secret is '{secretValue}'.");
                
                Console.WriteLine($"Retrieving your secret from {keyVaultName}.");
                
                // Get secret
                KeyVaultSecret secret = await client.GetSecretAsync(secretName);
                Console.WriteLine($"Your secret is '{secret.Value}'.");
                
                // Optional: Uncomment below if you want to delete the secret
                /*
                Console.Write($"Deleting your secret from {keyVaultName} ...");
                var deleteOperation = await client.StartDeleteSecretAsync(secretName);
                await deleteOperation.WaitForCompletionAsync();
                Console.WriteLine(" done.");
                
                await Task.Delay(5000); // Wait for deletion to propagate
                
                Console.Write($"Purging your secret from {keyVaultName} ...");
                await client.PurgeDeletedSecretAsync(secretName);
                Console.WriteLine(" done.");
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($" failed.");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}