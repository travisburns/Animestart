namespace animestart.Models
{
    public class Anime
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public int Rating { get; set; }


        public string Era { get; set; }
        public string ContentWarnings { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public ICollection<UserRecommendation> Recommendations { get; set; }
    }

}
