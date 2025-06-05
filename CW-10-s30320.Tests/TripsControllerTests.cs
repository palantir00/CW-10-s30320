using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CW_10_s30320.Controllers;    
using CW_10_s30320.Data;           
using CW_10_s30320.Models;         
using CW_10_s30320.DTOs;           
namespace CW_10_s30320.Tests
{
   public class TripsControllerTests
   {
       private MasterContext GetInMemoryContext(string dbName)
       {
           var options = new DbContextOptionsBuilder<MasterContext>()
               .UseInMemoryDatabase(databaseName: dbName)
               .Options;
           var context = new MasterContext(options);

           context.Trips.Add(new Trip { Id = 1, Destination = "Wrocław", Price = 1000 });
           context.SaveChanges();
           return context;
       }
       [Fact]
       public async Task GetTrip_ExistingId_ShouldReturnOkAndTrip()
       {
           var db = GetInMemoryContext("GetTripTestDb");
           var controller = new TripsController(db);

           var actionResult = await controller.GetTrip(1);
           var okResult = actionResult as OkObjectResult;
           okResult.Should().NotBeNull();
           var trip = okResult.Value as Trip;
           trip.Should().NotBeNull();
           trip.Id.Should().Be(1);
           trip.Destination.Should().Be("Wrocław");
       }
       [Fact]
       public async Task GetTrip_NonExistingId_ShouldReturnNotFound()
       {
  
           var db = GetInMemoryContext("GetTripNotFoundDb");
           var controller = new TripsController(db);

           var actionResult = await controller.GetTrip(999);
           var notFoundResult = actionResult as NotFoundResult;
           notFoundResult.Should().NotBeNull();
       }
       [Fact]
       public async Task DeleteTrip_ExistingId_ShouldReturnNoContent()
       {

           var db = GetInMemoryContext("DeleteTripTestDb");
           var controller = new TripsController(db);

           var actionResult = await controller.Delete(1);
           var noContent = actionResult as NoContentResult;
           noContent.Should().NotBeNull();

           db.Trips.Any(t => t.Id == 1).Should().BeFalse();
       }
       [Fact]
       public async Task DeleteTrip_NonExistingId_ShouldReturnBadRequest()
       {

           var db = GetInMemoryContext("DeleteTripBadRequestDb");
           var controller = new TripsController(db);

           var actionResult = await controller.Delete(12345);

           var badRequest = actionResult as BadRequestObjectResult;
           badRequest.Should().NotBeNull();

           var msg = badRequest.Value.ToString().ToLower();
           msg.Should().Contain("not found");
       }
       [Fact]
       public async Task CreateTrip_InvalidModel_ShouldReturnBadRequest()
       {

           var db = GetInMemoryContext("CreateTripInvalidDb");
           var controller = new TripsController(db);

           var invalidDto = new CreateTripDto
           {
               Destination = "",    
               Price = -100         
           };

           var actionResult = await controller.Create(invalidDto);
           var badRequest = actionResult as BadRequestObjectResult;
           badRequest.Should().NotBeNull();
           var msg = badRequest.Value.ToString().ToLower();
           msg.Should().Contain("invalid");
       }
       [Fact]
       public async Task CreateTrip_ValidModel_ShouldReturnCreated()
       {

           var db = GetInMemoryContext("CreateTripValidDb");
           var controller = new TripsController(db);
           var validDto = new CreateTripDto
           {
               Destination = "Warszawa",
               Price = 2500
           };

           var actionResult = await controller.Create(validDto);
           var createdResult = actionResult as CreatedAtActionResult;
           createdResult.Should().NotBeNull();
           var createdTrip = createdResult.Value as Trip;
           createdTrip.Should().NotBeNull();
           createdTrip.Id.Should().BeGreaterThan(0);
           createdTrip.Destination.Should().Be("Warszawa");
       }
   }
}