using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KunFarm.Presentation.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;
        private readonly IUserManagementService _userService;

        public UsersModel(ILogger<UsersModel> logger, IUserManagementService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        // Properties bound to the UI

        // Properties bound to the UI
        public IList<UserListResponse> Users { get; private set; } = new List<UserListResponse>();
        public int TotalUsers { get; private set; }
        public int ActiveUsers { get; private set; }
        public int InactiveUsers { get; private set; }
        public int OnlineUsers { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Admin Users page accessed at {Time}", DateTime.UtcNow);

            // Load statistics
            TotalUsers = await _userService.GetTotalUsersCountAsync();
            ActiveUsers = await _userService.GetActiveUsersCountAsync();
            InactiveUsers = TotalUsers - ActiveUsers;
            //OnlineUsers = await _userService.();

            // Load filtered and searched users

            Users = await _userService.GetAllUsersAsync();
		}

        //public async Task<IActionResult> OnPostExportAsync()
        //{
        //    var exportData = await _userService.GetUsersForExportAsync(SearchTerm, RoleFilter, StatusFilter);
        //    var fileContent = Exce.ExportUsers(exportData);

        //    _logger.LogInformation("Exported {Count} users to Excel", exportData.Count);
        //    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        //}

        public IActionResult OnPostRefresh()
        {
            // Redirect to refresh the page
            return RedirectToPage();
        }
    }
} 