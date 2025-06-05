using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CW_10_s30320.Data;
using CW_10_s30320.DTOs;
using CW_10_s30320.Models;
namespace CW_10_s30320.Controllers
{
   [ApiController]
   [Route("api/trips")]
   public class TripsController : ControllerBase
   {
       private readonly MasterContext _context;
       public TripsController(MasterContext context)
       {
           _context = context;
       }
       // DELETE /api/trips/{id}
       [HttpDelete("{id:int}")]
       public async Task<IActionResult> Delete(int id)
       {
           var trip = await _context.Trips.FindAsync(id);
           if (trip == null)
               return NotFound();
           bool hasClients = await _context.Client_Trips.AnyAsync(ct => ct.IdTrip == id);
           if (hasClients)
               return BadRequest("Nie można usunąć, ponieważ istnieją przypisani klienci.");
           _context.Trips.Remove(trip);
           await _context.SaveChangesAsync();
           return Ok();
       }
       // POST /api/trips/{tripId}/clients
       [HttpPost("{tripId:int}/clients")]
       public async Task<IActionResult> AddClientToTrip(int tripId, CreateClientInTripDto dto)
       {
           var trip = await _context.Trips.FindAsync(tripId);
           if (trip == null)
               return NotFound();
           if (trip.DateFrom < DateTime.UtcNow)
               return BadRequest("Nie można zapisać, ponieważ wycieczka już się odbyła.");
           // poszukujemy, czy klient o danym peselu już istnieje
           var existingClient = await _context.Clients.SingleOrDefaultAsync(c => c.Pesel == dto.Pesel);
           if (existingClient != null)
           {
               // sprawdzamy, czy klient jest już przypisany do tej wycieczki
               bool already = await _context.Client_Trips.AnyAsync(ct =>
                   ct.IdTrip == tripId && ct.IdClient == existingClient.IdClient);
               if (already)
                   return BadRequest("Duplikat: klient o takim PESELu jest już zapisany na tę wycieczkę.");
               // w przeciwnym razie przypisujemy go
               var clientTrip = new Client_Trip
               {
                   IdTrip = tripId,
                   IdClient = existingClient.IdClient,
                   RegistrationDate = dto.RegistrationDate,
                   PaymentDate = dto.PaymentDate
               };
               _context.Client_Trips.Add(clientTrip);
               await _context.SaveChangesAsync();
               return Ok();
           }
           else
           {
               // tworzymy nowego klienta i przypisujemy
               var newClient = new Client
               {
                   Pesel = dto.Pesel,
                   FirstName = dto.FirstName,
                   LastName = dto.LastName,
                   Email = dto.Email,
                   Telephone = dto.Telephone
               };
               _context.Clients.Add(newClient);
               await _context.SaveChangesAsync();
               var clientTrip = new Client_Trip
               {
                   IdTrip = tripId,
                   IdClient = newClient.IdClient,
                   RegistrationDate = dto.RegistrationDate,
                   PaymentDate = dto.PaymentDate
               };
               _context.Client_Trips.Add(clientTrip);
               await _context.SaveChangesAsync();
               return Ok();
           }
       }
   }
}