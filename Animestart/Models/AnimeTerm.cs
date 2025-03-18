using System.ComponentModel.DataAnnotations;

namespace animestart.Models
{
    public class AnimeTerm
    {
        public int Id { get; set; }

        [Required]
        public string Term { get; set; }

        [Required]
        public string Definition { get; set; }

        private string _category;

        [Required]
        public string Category
        {
            get { return _category ?? string.Empty; }
            set { _category = value; }
        }
    }
}