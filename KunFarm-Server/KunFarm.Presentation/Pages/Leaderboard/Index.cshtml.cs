using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KunFarm.Presentation.Pages.Leaderboard
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Page initialization logic here
            _logger.LogInformation("Leaderboard page accessed");
        }
    }
} 