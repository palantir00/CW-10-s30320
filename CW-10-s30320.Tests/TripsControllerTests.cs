using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CW_10_s30320.Data;
using CW_10_s30320.Controllers;
using CW_10_s30320.Models;
using CW_10_s30320.DTOs;
namespace CW_10_s30320.Tests
{
   public class TripsControllerTests
   {
       private DbContextOptions<MasterContext> CreateNewContextOptions()
       {
           return new DbContextOptionsBuilder<MasterContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;
       }
       [Fact(DisplayName = "Delete Trip with assigned clients → 400 BadRequest")]
       public async Task Delete_Trip_With_Clients_Returns_BadRequest()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               var trip = new Trip
               {
                   Name = "Test Trip",
                   Description = "Desc",
                   DateFrom = DateTime.UtcNow.AddDays(10),
                   DateTo = DateTime.UtcNow.AddDays(15),
                   MaxPeople = 20
               };
               context.Trips.Add(trip);
               context.SaveChanges();
               var client = new Client
               {
                   Pesel = "12345678901",
                   FirstName = "Jan",
                   LastName = "Kowalski",
                   Email = "jan@example.com",
                   Telephone = "123456789"
               };
               context.Clients.Add(client);
               context.SaveChanges();
               var clientTrip = new Client_Trip
               {
                   IdTrip = trip.IdTrip,
                   IdClient = client.IdClient,
                   RegistrationDate = DateTime.UtcNow,
                   PaymentDate = null
               };
               context.Client_Trips.Add(clientTrip);
               context.SaveChanges();
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var result = await controller.Delete(1);
               result.Should().BeOfType<BadRequestObjectResult>();
               var badReq = (BadRequestObjectResult)result;
               badReq.Value.ToString().Should().ContainEquivalentOf("przypisani klienci");
           }
       }
       [Fact(DisplayName = "Delete Trip without any clients → 200 OK")]
       public async Task Delete_Trip_Without_Clients_Returns_Ok()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               var trip = new Trip
               {
                   Name = "Solo Trip",
                   Description = "No clients here",
                   DateFrom = DateTime.UtcNow.AddDays(5),
                   DateTo = DateTime.UtcNow.AddDays(7),
                   MaxPeople = 10
               };
               context.Trips.Add(trip);
               context.SaveChanges();
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var result = await controller.Delete(1);
               result.Should().BeOfType<OkResult>();
               context.Trips.Any(t => t.IdTrip == 1).Should().BeFalse();
           }
       }
       [Fact(DisplayName = "Delete Non-Existing Trip → 404 NotFound")]
       public async Task Delete_NonExisting_Trip_Returns_NotFound()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               // brak tripów w bazie
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var result = await controller.Delete(42);
               result.Should().BeOfType<NotFoundResult>();
           }
       }
       [Fact(DisplayName = "AddClientToTrip: duplicate PESEL in same trip → 400 BadRequest")]
       public async Task AddClientToTrip_Duplicate_Pesel_Returns_BadRequest()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               var trip = new Trip
               {
                   Name = "TripDupPesel",
                   Description = "Future trip",
                   DateFrom = DateTime.UtcNow.AddDays(2),
                   DateTo = DateTime.UtcNow.AddDays(4),
                   MaxPeople = 5
               };
               context.Trips.Add(trip);
               context.SaveChanges();
               var client = new Client
               {
                   Pesel = "98765432100",
                   FirstName = "Anna",
                   LastName = "Nowak",
                   Email = "anna@foo.com",
                   Telephone = "555444333"
               };
               context.Clients.Add(client);
               context.SaveChanges();
               var clientTrip = new Client_Trip
               {
                   IdTrip = trip.IdTrip,
                   IdClient = client.IdClient,
                   RegistrationDate = DateTime.UtcNow,
                   PaymentDate = null
               };
               context.Client_Trips.Add(clientTrip);
               context.SaveChanges();
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var dto = new CreateClientInTripDto
               {
                   Pesel = "98765432100",
                   FirstName = "Anna",
                   LastName = "Nowak",
                   Email = "anna@foo.com",
                   Telephone = "555444333",
                   RegistrationDate = DateTime.UtcNow,
                   PaymentDate = null
               };
               var result = await controller.AddClientToTrip(1, dto);
               result.Should().BeOfType<BadRequestObjectResult>();
               var badReq = (BadRequestObjectResult)result;
               badReq.Value.ToString().Should().ContainEquivalentOf("duplikat");
           }
       }
       [Fact(DisplayName = "AddClientToTrip: trip date in past → 400 BadRequest")]
       public async Task AddClientToTrip_TripInPast_Returns_BadRequest()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               var trip = new Trip
               {
                   Name = "PastTrip",
                   Description = "Already happened",
                   DateFrom = DateTime.UtcNow.AddDays(-10),
                   DateTo = DateTime.UtcNow.AddDays(-5),
                   MaxPeople = 3
               };
               context.Trips.Add(trip);
               context.SaveChanges();
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var dto = new CreateClientInTripDto
               {
                   Pesel = "11223344556",
                   FirstName = "Piotr",
                   LastName = "Zalewski",
                   Email = "piotr@bar.com",
                   Telephone = "111222333",
                   RegistrationDate = DateTime.UtcNow,
                   PaymentDate = null
               };
               var result = await controller.AddClientToTrip(1, dto);
               result.Should().BeOfType<BadRequestObjectResult>();
               var badReq = (BadRequestObjectResult)result;
               badReq.Value.ToString().Should().ContainEquivalentOf("wycieczka już się odbyła");
           }
       }
       [Fact(DisplayName = "AddClientToTrip: new client → 200 OK")]
       public async Task AddClientToTrip_NewClient_Succeeds_Returns_Ok()
       {
           var options = CreateNewContextOptions();
           using (var context = new MasterContext(options))
           {
               var trip = new Trip
               {
                   Name = "NoClientsTrip",
                   Description = "Can add",
                   DateFrom = DateTime.UtcNow.AddDays(3),
                   DateTo = DateTime.UtcNow.AddDays(6),
                   MaxPeople = 5
               };
               context.Trips.Add(trip);
               context.SaveChanges();
           }
           using (var context = new MasterContext(options))
           {
               var controller = new TripsController(context);
               var dto = new CreateClientInTripDto
               {
                   Pesel = "55566677788",
                   FirstName = "Marek",
                   LastName = "Kowal",
                   Email = "marek@baz.com",
                   Telephone = "987654321",
                   RegistrationDate = DateTime.UtcNow,
                   PaymentDate = null
               };
               var result = await controller.AddClientToTrip(1, dto);
               result.Should().BeOfType<OkResult>();
               var createdClient = context.Clients.SingleOrDefault(c => c.Pesel == "55566677788");
               createdClient.Should().NotBeNull();
               var clientTrip = context.Client_Trips
                   .SingleOrDefault(ct => ct.IdTrip == 1 && ct.IdClient == createdClient!.IdClient);
               clientTrip.Should().NotBeNull();
               clientTrip!.RegistrationDate.Should().Be(dto.RegistrationDate);
               clientTrip.PaymentDate.Should().BeNull();
           }
       }
   }
}