namespace animestart.Models
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<UserRecommendation> Recommendations { get; set; }
        public List<UserWatchlistItem> WatchlistItems { get; set; }
        public string ActiveTab { get; set; }
    }
}