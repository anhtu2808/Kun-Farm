namespace KunFarm.BLL.DTOs.Response
{
    public class LeaderboardResponse
    {
        public List<LeaderboardEntry> Rankings { get; set; } = new List<LeaderboardEntry>();
        public int TotalPlayers { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
    
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Money { get; set; }
        public DateTime LastSaved { get; set; }
        public bool IsActive { get; set; }
    }
} 