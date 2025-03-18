using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using animestart.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public virtual DbSet<Anime> Animes { get; set; }
    public virtual DbSet<AnimeTerm> AnimeTerms { get; set; }
    public virtual DbSet<StarterPack> StarterPacks { get; set; }
    public virtual DbSet<UserRecommendation> UserRecommendations { get; set; }

    public virtual DbSet<UserWatchlistItem> WatchlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var hasher = new PasswordHasher<ApplicationUser>();
        var adminUser = new ApplicationUser
        {
            Id = "1",
            UserName = "admin@animebeginners.com",
            NormalizedUserName = "ADMIN@ANIMEBEGINNERS.COM",
            Email = "admin@animebeginners.com",
            NormalizedEmail = "ADMIN@ANIMEBEGINNERS.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            FirstName = "Admin",
            LastName = "User",
            RegistrationDate = DateTime.Now
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

        // Seed the Administrator role
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = RoleConstants.Administrator,
                NormalizedName = RoleConstants.Administrator.ToUpper()
            }
        );

        // Connect the user to the Administrator role
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "1",
                UserId = "1"
            }
        );

        // Anime configuration
        modelBuilder.Entity<Anime>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(50);


            // One Anime can have many recommendations
            entity.HasMany(a => a.Recommendations)
                  .WithOne(r => r.Anime)
                  .HasForeignKey(r => r.AnimeId);
        });

        // AnimeTerm configuration
        modelBuilder.Entity<AnimeTerm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Term).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Definition).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
        });

        // StarterPack configuration
        modelBuilder.Entity<StarterPack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();

        });

        // UserRecommendation configuration
        modelBuilder.Entity<UserRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.Rating).IsRequired();

            // Each recommendation must be linked to a user
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .IsRequired();
        });

        modelBuilder.Entity<Anime>().HasData(
            new Anime
            {
                Id = 1,
                Title = "Demon Slayer",
                Description = "A boy becomes a demon slayer after his family is killed and his sister turned into a demon.",
                Genre = "Shonen",
                Year = 2019,
                Rating = 5,
                Era = "Modern",
                ImagePath =""
            },
            new Anime
            {
                Id = 2,
                Title = "Death Note",
                Description = "A high school student discovers a supernatural notebook that allows him to kill anyone by writing the victim's name.",
                Genre = "Psychological Thriller",
                Year = 2006,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 3,
                Title = "Attack on Titan",
                Description = "Humanity lives inside cities surrounded by enormous walls due to the Titans, gigantic humanoid creatures who devour humans.",
                Genre = "Dark Fantasy",
                Year = 2013,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 4,
                Title = "My Hero Academia",
                Description = "A boy born without superpowers in a world where they are common hopes to become a hero.",
                Genre = "Shonen",
                Year = 2016,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 5,
                Title = "One Punch Man",
                Description = "A superhero who can defeat any opponent with a single punch seeks a worthy opponent.",
                Genre = "Action Comedy",
                Year = 2015,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 6,
                Title = "Naruto",
                Description = "A young ninja seeks recognition from his peers and dreams of becoming the Hokage.",
                Genre = "Shonen",
                Year = 2002,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 7,
                Title = "Fullmetal Alchemist: Brotherhood",
                Description = "Two brothers seek the Philosopher's Stone to restore their bodies after a failed alchemical ritual.",
                Genre = "Adventure",
                Year = 2009,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            },
            new Anime
            {
                Id = 8,
                Title = "Steins;Gate",
                Description = "A self-proclaimed mad scientist discovers a way to send messages to the past.",
                Genre = "Sci-Fi Thriller",
                Year = 2011,
                Rating = 5,
                Era = "Modern",
                ImagePath = ""
            }
        );

        // Seed AnimeTerm data
        modelBuilder.Entity<AnimeTerm>().HasData(
            new AnimeTerm
            {
                Id = 1,
                Term = "Shonen",
                Definition = "Anime and manga targeted primarily at teenage boys, typically featuring action-packed stories with strong protagonists.",
                Category = "Genre"
            },
            new AnimeTerm
            {
                Id = 2,
                Term = "Shoujo",
                Definition = "Anime and manga targeted primarily at teenage girls, often featuring romance and personal growth stories.",
                Category = "Genre"
            },
            new AnimeTerm
            {
                Id = 3,
                Term = "Seinen",
                Definition = "Anime and manga targeted at adult men, often featuring more complex themes and mature content.",
                Category = "Genre"
            },
            new AnimeTerm
            {
                Id = 4,
                Term = "Ecchi",
                Definition = "Anime and manga with mild sexual content or fan service. Important to note for new viewers who may be uncomfortable with this content.",
                Category = "Content Warning"
            },
            new AnimeTerm
            {
                Id = 5,
                Term = "Mecha",
                Definition = "Anime featuring giant robots or mechanical suits, often with sci-fi themes.",
                Category = "Genre"
            },
            new AnimeTerm
            {
                Id = 6,
                Term = "Isekai",
                Definition = "Stories where protagonists are transported to another world, often fantasy settings.",
                Category = "Genre"
            },
            new AnimeTerm
            {
                Id = 7,
                Term = "The Big Three",
                Definition = "Refers to three highly popular anime series: Naruto, Bleach, and One Piece, which dominated the 2000s era.",
                Category = "Historical Term"
            },
            new AnimeTerm
            {
                Id = 8,
                Term = "Slice of Life",
                Definition = "Anime focusing on everyday life experiences, often with minimal conflict or drama.",
                Category = "Genre"
            }
        );

        // Seed StarterPack data
        modelBuilder.Entity<StarterPack>().HasData(
            new StarterPack
            {
                Id = 1,
                Name = "Beginner Action Pack",
                Description = "Perfect for newcomers who enjoy action and adventure. Includes popular shows with straightforward plots and engaging characters.",

            },
            new StarterPack
            {
                Id = 2,
                Name = "Psychological Thriller Starter",
                Description = "For viewers who enjoy complex plots and mind-bending stories. These shows require more attention but offer deep narratives.",

            },
            new StarterPack
            {
                Id = 3,
                Name = "Classic Anime Essentials",
                Description = "A collection of must-watch classics that have shaped modern anime. Great for understanding anime's evolution.",

            },
            new StarterPack
            {
                Id = 4,
                Name = "Modern Masterpieces",
                Description = "Recent high-quality shows that showcase the best of modern animation and storytelling.",

            }
        );

       
        modelBuilder.Entity<UserRecommendation>().HasData(
            new UserRecommendation
            {
                Id = 1,
                AnimeId = 1, // Demon Slayer
                UserId = "1", 
                Comment = "Perfect starter anime with amazing animation and straightforward story.",
                Rating = 5
            },
            new UserRecommendation
            {
                Id = 2,
                AnimeId = 4, // My Hero Academia
                UserId = "1",
                Comment = "Great introduction to shonen anime with western superhero elements.",
                Rating = 4
            }

        );
    }
}