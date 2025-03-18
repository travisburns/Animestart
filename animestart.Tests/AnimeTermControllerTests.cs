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
    public class AnimeTermControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public AnimeTermControllerTests()
        {
            // Creatin options for in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create context with those options
            _context = new ApplicationDbContext(options);

            // Add test data
            _context.AnimeTerms.Add(new AnimeTerm
            {
                Id = 1,
                Term = "Shonen",
                Definition = "Anime for young boys",
                Category = "Genre"
            });
            _context.AnimeTerms.Add(new AnimeTerm
            {
                Id = 2,
                Term = "Shoujo",
                Definition = "Anime for young girls",
                Category = "Genre"
            });
            _context.AnimeTerms.Add(new AnimeTerm
            {
                Id = 3,
                Term = "Seinen",
                Definition = "Anime for adult men",
                Category = "Demographic"
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllTerms()
        {
            // Arrange
            var controller = new AnimeTermController(_context);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AnimeTerm>>(viewResult.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_WithCategoryFilter_ReturnsFilteredResults()
        {
            // Arrange
            var controller = new AnimeTermController(_context);

            // Act
            var result = await controller.Index("Genre");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AnimeTerm>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.All(model, term => Assert.Equal("Genre", term.Category));
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithTerm()
        {
            // Arrange
            var controller = new AnimeTermController(_context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AnimeTerm>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Shonen", model.Term);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new AnimeTermController(_context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}