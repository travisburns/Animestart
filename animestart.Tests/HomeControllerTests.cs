using animestart.Controllers;
using animestart.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace animestart.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        public HomeControllerTests()
        {
            // Create a in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Add test data
            _context.Animes.AddRange(
                new Anime { Id = 1, Title = "Test Anime 1", Description = "Description 1", Genre = "Action", Year = 2020, Rating = 5, Era = "Modern" },
                new Anime { Id = 2, Title = "Test Anime 2", Description = "Description 2", Genre = "Comedy", Year = 2019, Rating = 4, Era = "Modern" },
                new Anime { Id = 3, Title = "Test Anime 3", Description = "Description 3", Genre = "Drama", Year = 2021, Rating = 3, Era = "Modern" }
            );

            _context.StarterPacks.Add(new StarterPack { Id = 1, Name = "Test Pack", Description = "Test Pack Description" });

            _context.AnimeTerms.AddRange(
                new AnimeTerm { Id = 1, Term = "Shonen", Definition = "For boys", Category = "Genre" },
                new AnimeTerm { Id = 2, Term = "Shoujo", Definition = "For girls", Category = "Genre" }
            );

            _context.SaveChanges();

            // Mock logger
            _mockLogger = new Mock<ILogger<HomeController>>();

            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock RoleManager
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _context,
                _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            var controller = new HomeController(_mockLogger.Object, _context,
                _mockUserManager.Object, _mockRoleManager.Object);

           
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Setup for Activity.Current which might be used in RequestId
            Activity.Current = new Activity("TestActivity").Start();

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ErrorViewModel>(viewResult.Model);

            // Clean up Activity
            Activity.Current?.Stop();
            Activity.Current = null;
        }
    }
}