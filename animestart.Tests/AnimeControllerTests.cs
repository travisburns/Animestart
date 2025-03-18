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
    public class AnimeControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public AnimeControllerTests()
        {
            // Create options for in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create context with those options
            _context = new ApplicationDbContext(options);

            // Add test data
            _context.Animes.Add(new Anime
            {
                Id = 1,
                Title = "Test Anime 1",
                Description = "Description 1",
                Genre = "Action",
                Year = 2020,
                Rating = 5,
                Era = "Modern"
            });
            _context.Animes.Add(new Anime
            {
                Id = 2,
                Title = "Test Anime 2",
                Description = "Description 2",
                Genre = "Comedy",
                Year = 2019,
                Rating = 4,
                Era = "Modern"
            });
            _context.Animes.Add(new Anime
            {
                Id = 3,
                Title = "Another Test",
                Description = "Description 3",
                Genre = "Action",
                Year = 2021,
                Rating = 3,
                Era = "Contemporary"
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsViewWithAnimeList()
        {
            // Arrange
            var controller = new AnimeController(_context);

            // Act
            var result = await controller.Index(null, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Anime>>(viewResult.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_WithTitleSearch_ReturnsFilteredResults()
        {
            // Arrange
            var controller = new AnimeController(_context);

            // Act
            var result = await controller.Index("Another", null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Anime>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Another Test", model[0].Title);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithAnime()
        {
            // Arrange
            var controller = new AnimeController(_context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Anime>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Anime 1", model.Title);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new AnimeController(_context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsViewResult()
        {
            // Arrange
            var controller = new AnimeController(_context);

            // Act
            var result = await controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var controller = new AnimeController(_context);
            var anime = new Anime
            {
                Title = "New Anime",
                Description = "New Description",
                Genre = "Action",
                Year = 2023,
                Rating = 5,
                Era = "Modern"
            };

            // Act
            var result = await controller.Create(anime);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify it was added to database
            var addedAnime = await _context.Animes.FirstOrDefaultAsync(a => a.Title == "New Anime");
            Assert.NotNull(addedAnime);
        }

        [Fact]
        public async Task Create_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var controller = new AnimeController(_context);
            var anime = new Anime(); 
            controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await controller.Create(anime);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(anime, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}