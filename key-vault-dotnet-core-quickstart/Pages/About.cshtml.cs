using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace key_vault_dotnet_core_quickstart.Pages
{
    public class AboutModel : PageModel
    {
        public string? Message { get; set; }
        private readonly IConfiguration _configuration;

        public AboutModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            // Use the correct secret name that exists in your Key Vault
         Message = "My key val = " + _configuration["AppSecret"];
         
                  
        }
    }
}