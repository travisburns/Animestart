namespace animestart.Models
{
    public class StarterPackAnime
    {
        public int Id { get; set; }
        public int StarterPackId { get; set; }
        public int AnimeId { get; set; }

        public StarterPack StarterPack { get; set; }
        public Anime Anime { get; set; }
    }
}