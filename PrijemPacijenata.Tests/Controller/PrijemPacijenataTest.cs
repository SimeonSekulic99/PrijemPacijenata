using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrijemPacijenata.Controllers;
using PrijemPacijenata.Data;
using PrijemPacijenata;
using Xunit;

namespace PrijemPacijenata.Tests
{
    public class PrijemPacijenataControllerTests
    {
        private readonly PrijemPacijenataController _controller;
        private readonly DataContext _context;

        public PrijemPacijenataControllerTests()
        {
            // Arrange - Set up the controller with a fake DataContext
            _context = A.Fake<DataContext>();
            _controller = new PrijemPacijenataController(_context);
        }

        [Fact]
        public async Task PrikaziSvePacijente_ReturnsOkResult()
        {
            // Arrange - Create a fake list of Pacijent
            var fakePacijenti = new List<Pacijent>
            {
                // Add your test data here
            };

            // Arrange - Set up the fake DbSet for context
            var fakeDbSet = A.Fake<DbSet<Pacijent>>();
            A.CallTo(() => _context.Pacijenti).Returns(fakeDbSet);

            // Use `Task.FromResult` for the inner task
            A.CallTo(() => fakeDbSet.Include(A<Expression<Func<Pacijent, object>>>._))
                .Returns(Task.FromResult(fakeDbSet));

            // Use `Task.FromResult` for the outer task
            A.CallTo(() => fakeDbSet.ToListAsync()).Returns(Task.FromResult(Task.FromResult(fakePacijenti)));

            // Act - Call the action method
            var result = await _controller.PrikaziSvePacijente();

            // Assert - Check the result
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pacijenti = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(fakePacijenti.Count, pacijenti.Count());
        }

        [Fact]
        public async Task DodajPacijenta_ValidModel_ReturnsOkResult()
        {
            // Arrange - Create a fake Pacijent model
            var fakePacijent = new Pacijent
            {
                // Add your test data here
            };

            // Arrange - Set up fake Doctor and other necessary dependencies

            // Act - Call the action method
            var result = await _controller.CreatePatient(fakePacijent);

            // Assert - Check the result
            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Equal("Pacijent uspesno unet! Odabrani lekar ima 0 pacijenata", message);
        }
    }
}
