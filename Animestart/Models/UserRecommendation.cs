namespace animestart.Models
{
    public class UserRecommendation
    {
        public int Id { get; set; }
        public int AnimeId { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }

        public Anime Anime { get; set; }
    }
}
