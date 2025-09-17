using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace akvdotnet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Program p = new Program();
            string secretName = "AppSecret";
            
            // kvURL must be updated to the URL of your key vault
            string kvURL = "https://rgvault254.vault.azure.net/";
            
            // Modern authentication using DefaultAzureCredential
            // This will use environment variables, managed identity, or Azure CLI
            var client = new SecretClient(new Uri(kvURL), new DefaultAzureCredential());
            
            Console.Write("Input the value of your secret > ");
            string secretValue = Console.ReadLine();
            
            Console.WriteLine($"Your secret is '{secretValue}'.");
            
            Console.Write("Saving the value of your secret to your key vault ...");
            
            // Set secret
            var result = await p.SetSecret(client, secretName, secretValue);
            
            await Task.Delay(2000); // Replace Thread.Sleep with async delay
            
            Console.WriteLine("done.");
            
            Console.WriteLine("Forgetting your secret.");
            secretValue = "";
            Console.WriteLine($"Your secret is '{secretValue}'.");
            Console.WriteLine("Retrieving your secret from key vault.");
            
            var fetchedSecret = await p.GetSecret(client, secretName);
            
            secretValue = fetchedSecret;
            Console.WriteLine($"Your secret is {secretValue}");
        }
        
        /// <summary>
        /// Sets a secret in the Key Vault
        /// </summary>
        /// <returns>True if successful</returns>
        public async Task<bool> SetSecret(SecretClient client, string secretName, string secretValue)
        {
            try
            {
                await client.SetSecretAsync(secretName, secretValue);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting secret: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets a secret from the Key Vault
        /// </summary>
        /// <returns>The secret value</returns>
        public async Task<string> GetSecret(SecretClient client, string secretName)
        {
            try
            {
                var keyvaultSecret = await client.GetSecretAsync(secretName);
                return keyvaultSecret.Value.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting secret: {ex.Message}");
                return string.Empty;
            }
        }
    }
}