using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KunFarm.Presentation.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(ILogger<UsersModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Page initialization logic here
            _logger.LogInformation("Admin Users page accessed");
        }
    }
} 