namespace animestart.Models
{
    public class UserWatchlistItem
    {
        public int Id { get; set; }
        public int AnimeId { get; set; }
        public string UserId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public Anime Anime { get; set; }
    }
}