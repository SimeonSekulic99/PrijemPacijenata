using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using PrijemPacijenata.Controllers;
using PrijemPacijenata.Migrations;
using PrijemPacijenata.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;


namespace PrijemPacijenata.Tests
{
    public class PrijemPacijenataControllerTests
    {
        [Fact]
        public async Task PrikaziSvePacijente_ReturnsOkResultWithPacijenti()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DataContext(dbContextOptions))
            {
                context.Pacijenti.AddRange(
                    new Pacijent { IDPacijenta = 1, Ime = "John", Prezime = "Doe", BrojSobe = 101 },
                    new Pacijent { IDPacijenta = 2, Ime = "Jane", Prezime = "Smith", BrojSobe = 102 }
                );
                context.SaveChanges();
            }

            using (var context = new DataContext(dbContextOptions))
            {
                var controller = new PrijemPacijenataController(context);

                // Act
                var result = await controller.PrikaziSvePacijente();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacijenti = Assert.IsType<List<object>>(okResult.Value);

                Assert.Equal(2, pacijenti.Count);

                var firstPacijent = Assert.IsType<Dictionary<string, object>>(pacijenti[0]);
                Assert.Equal(1, firstPacijent["IDPacijenta"]);
                Assert.Equal("John", firstPacijent["Ime"]);
                Assert.Equal("Doe", firstPacijent["Prezime"]);
                Assert.Equal(101, firstPacijent["BrojSobe"]);

                var secondPacijent = Assert.IsType<Dictionary<string, object>>(pacijenti[1]);
                Assert.Equal(2, secondPacijent["IDPacijenta"]);
                Assert.Equal("Jane", secondPacijent["Ime"]);
                Assert.Equal("Smith", secondPacijent["Prezime"]);
                Assert.Equal(102, secondPacijent["BrojSobe"]);
            }
        }
    }
}
