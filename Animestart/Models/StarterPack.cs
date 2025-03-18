namespace animestart.Models
{
    public class StarterPack
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Anime> Animes { get; set; } = new List<Anime>();

    }
}
