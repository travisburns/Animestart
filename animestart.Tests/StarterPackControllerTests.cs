using animestart.Controllers;
using animestart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace animestart.Tests.Controllers
{
    public class StarterPackControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public StarterPackControllerTests()
        {
            // Creation of options for in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create context with those options
            _context = new ApplicationDbContext(options);

            // Add test anime data
            var anime1 = new Anime
            {
                Id = 1,
                Title = "Test Anime 1",
                Description = "Description 1",
                Genre = "Action",
                Year = 2020,
                Rating = 3,
                Era = "Modern"
            };
            var anime2 = new Anime
            {
                Id = 2,
                Title = "Test Anime 2",
                Description = "Description 2",
                Genre = "Comedy",
                Year = 2019,
                Rating = 4,
                Era = "Modern"
            };
            var anime3 = new Anime
            {
                Id = 3,
                Title = "Test Anime 3",
                Description = "Description 3",
                Genre = "Drama",
                Year = 2021,
                Rating = 5,
                Era = "Modern"
            };
            var anime4 = new Anime
            {
                Id = 4,
                Title = "Test Anime 4",
                Description = "Description 4",
                Genre = "Action",
                Year = 2018,
                Rating = 2,
                Era = "Modern"
            };

            _context.Animes.AddRange(anime1, anime2, anime3, anime4);
            _context.SaveChanges();

            // Add test starter pack data
            var pack1 = new StarterPack
            {
                Id = 1,
                Name = "Test Pack 1",
                Description = "Description 1",
                Animes = new List<Anime> { anime1, anime2 }
            };
            var pack2 = new StarterPack
            {
                Id = 2,
                Name = "Test Pack 2",
                Description = "Description 2",
                Animes = new List<Anime> { anime3 }
            };
            var pack3 = new StarterPack
            {
                Id = 3,
                Name = "Empty Pack",
                Description = "No animes",
                Animes = new List<Anime>()
            };

            _context.StarterPacks.AddRange(pack1, pack2, pack3);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllStarterPacks()
        {
            // Arrange
            var controller = new StarterPackController(_context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<StarterPack>>(viewResult.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithStarterPack()
        {
            // Arrange
            var controller = new StarterPackController(_context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StarterPack>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Pack 1", model.Name);
            Assert.Equal(2, model.Animes.Count);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new StarterPackController(_context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WithEmptyAnimesCollection_SetsRecommendedAnime()
        {
            // Arrange
            var controller = new StarterPackController(_context);

            // Act
            var result = await controller.Details(3); 

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            // Check ViewBag.RecommendedAnime is set
            var recommendedAnime = viewResult.ViewData["RecommendedAnime"] as List<Anime>;
            Assert.NotNull(recommendedAnime);

           
            Assert.Contains(recommendedAnime, a => a.Rating <= 3);
        }
    }
}