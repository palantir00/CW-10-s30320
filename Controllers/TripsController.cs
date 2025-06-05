using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CW_10_s30320.Data;
using CW_10_s30320.DTOs;
using CW_10_s30320.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<PaginatedTripsDto>> GetTrips([FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNum < 1 || pageSize < 1)
                return BadRequest("pageNum and pageSize must be positive integers.");

            var totalTrips = await _context.Trips.CountAsync();
            var allPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

            var trips = await _context.Trips
                .Include(t => t.Country_Trips)
                    .ThenInclude(ct => ct.IdCountryNavigation)
                .Include(t => t.Client_Trips)
                    .ThenInclude(ct => ct.IdClientNavigation)
                .OrderByDescending(t => t.DateFrom)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var tripDtos = trips.Select(t => new TripDto
            {
                IdTrip = t.IdTrip,
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.Country_Trips
                              .Select(ct => new CountryDto
                              {
                                  IdCountry = ct.IdCountry,
                                  Name = ct.IdCountryNavigation.Name
                              })
                              .ToList(),
                Clients = t.Client_Trips
                           .Select(ct => new ClientDto
                           {
                               IdClient = ct.IdClient,
                               Pesel = ct.IdClientNavigation.Pesel,
                               FirstName = ct.IdClientNavigation.FirstName,
                               LastName = ct.IdClientNavigation.LastName,
                               Email = ct.IdClientNavigation.Email,
                               Telephone = ct.IdClientNavigation.Telephone
                           })
                           .ToList()
            }).ToList();

            var result = new PaginatedTripsDto
            {
                PageNum = pageNum,
                PageSize = pageSize,
                AllPages = allPages,
                Trips = tripDtos
            };

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TripDto>> GetTripById(int id)
        {
            var t = await _context.Trips
                .Include(ti => ti.Country_Trips)
                    .ThenInclude(ct => ct.IdCountryNavigation)
                .Include(ti => ti.Client_Trips)
                    .ThenInclude(ct => ct.IdClientNavigation)
                .FirstOrDefaultAsync(ti => ti.IdTrip == id);

            if (t == null) return NotFound();

            var dto = new TripDto
            {
                IdTrip = t.IdTrip,
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.Country_Trips
                              .Select(ct => new CountryDto
                              {
                                  IdCountry = ct.IdCountry,
                                  Name = ct.IdCountryNavigation.Name
                              }).ToList(),
                Clients = t.Client_Trips
                           .Select(ct => new ClientDto
                           {
                               IdClient = ct.IdClient,
                               Pesel = ct.IdClientNavigation.Pesel,
                               FirstName = ct.IdClientNavigation.FirstName,
                               LastName = ct.IdClientNavigation.LastName,
                               Email = ct.IdClientNavigation.Email,
                               Telephone = ct.IdClientNavigation.Telephone
                           }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<TripDto>> CreateTrip([FromBody] CreateTripDto newTrip)
        {
            if (newTrip.DateFrom >= newTrip.DateTo)
                return BadRequest("DateFrom must be earlier than DateTo.");

            var countriesList = await _context.Countries
                .Where(c => newTrip.IdCountries.Contains(c.IdCountry))
                .ToListAsync();

            if (countriesList.Count != newTrip.IdCountries.Count)
                return BadRequest("One or more specified countries do not exist.");

            var trip = new Trip
            {
                Name = newTrip.Name,
                Description = newTrip.Description,
                DateFrom = newTrip.DateFrom,
                DateTo = newTrip.DateTo,
                MaxPeople = newTrip.MaxPeople
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            foreach (var cId in newTrip.IdCountries)
            {
                _context.Country_Trips.Add(new Country_Trip
                {
                    IdTrip = trip.IdTrip,
                    IdCountry = cId
                });
            }
            await _context.SaveChangesAsync();

            var tripDto = new TripDto
            {
                IdTrip = trip.IdTrip,
                Name = trip.Name,
                Description = trip.Description,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                MaxPeople = trip.MaxPeople,
                Countries = countriesList.Select(c => new CountryDto
                {
                    IdCountry = c.IdCountry,
                    Name = c.Name
                }).ToList(),
                Clients = new List<ClientDto>()
            };

            return CreatedAtAction(nameof(GetTripById), new { id = trip.IdTrip }, tripDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] CreateTripDto updated)
        {
            var tripInDb = await _context.Trips.FindAsync(id);
            if (tripInDb == null) return NotFound();

            if (updated.DateFrom >= updated.DateTo)
                return BadRequest("DateFrom must be earlier than DateTo.");

            var countriesList = await _context.Countries
                .Where(c => updated.IdCountries.Contains(c.IdCountry))
                .ToListAsync();
            if (countriesList.Count != updated.IdCountries.Count)
                return BadRequest("One or more specified countries do not exist.");

            tripInDb.Name = updated.Name;
            tripInDb.Description = updated.Description;
            tripInDb.DateFrom = updated.DateFrom;
            tripInDb.DateTo = updated.DateTo;
            tripInDb.MaxPeople = updated.MaxPeople;

            var existingCountryTrips = await _context.Country_Trips
                .Where(ct => ct.IdTrip == id)
                .ToListAsync();

            foreach (var ex in existingCountryTrips)
            {
                if (!updated.IdCountries.Contains(ex.IdCountry))
                {
                    _context.Country_Trips.Remove(ex);
                }
            }

            var existingIds = existingCountryTrips.Select(ct => ct.IdCountry).ToHashSet();
            foreach (var newCountryId in updated.IdCountries)
            {
                if (!existingIds.Contains(newCountryId))
                {
                    _context.Country_Trips.Add(new Country_Trip
                    {
                        IdTrip = id,
                        IdCountry = newCountryId
                    });
                }
            }

            _context.Entry(tripInDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var t = await _context.Trips
                .Include(ti => ti.Client_Trips)
                .Include(ti => ti.Country_Trips)
                .FirstOrDefaultAsync(ti => ti.IdTrip == id);

            if (t == null) return NotFound();

            _context.Client_Trips.RemoveRange(t.Client_Trips);
            _context.Country_Trips.RemoveRange(t.Country_Trips);
            _context.Trips.Remove(t);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{tripId:int}/countries/{countryId:int}")]
        public async Task<IActionResult> AddCountryToTrip(int tripId, int countryId)
        {
            var t = await _context.Trips.FindAsync(tripId);
            if (t == null) return NotFound($"Trip with id {tripId} not found.");

            var c = await _context.Countries.FindAsync(countryId);
            if (c == null) return NotFound($"Country with id {countryId} not found.");

            bool already = await _context.Country_Trips
                .AnyAsync(ct => ct.IdTrip == tripId && ct.IdCountry == countryId);
            if (already) return BadRequest($"Trip {tripId} already has country {countryId}.");

            _context.Country_Trips.Add(new Country_Trip
            {
                IdTrip = tripId,
                IdCountry = countryId
            });
            await _context.SaveChangesAsync();
            return StatusCode(201);
        }

        [HttpPost("{tripId:int}/clients")]
        public async Task<IActionResult> AddClientToTrip(int tripId, [FromBody] CreateClientInTripDto newClientDto)
        {
            var trip = await _context.Trips
                .Include(t => t.Client_Trips)
                .FirstOrDefaultAsync(t => t.IdTrip == tripId);
            if (trip == null) return NotFound($"Trip with id {tripId} not found.");

            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Pesel == newClientDto.Pesel);

            if (existingClient != null)
            {
                bool alreadyAssigned = await _context.Client_Trips
                    .AnyAsync(ct => ct.IdTrip == tripId && ct.IdClient == existingClient.IdClient);
                if (alreadyAssigned)
                    return BadRequest($"Client {existingClient.IdClient} is already assigned to trip {tripId}.");
            }
            else
            {
                existingClient = new Client
                {
                    Pesel = newClientDto.Pesel,
                    FirstName = newClientDto.FirstName,
                    LastName = newClientDto.LastName,
                    Email = newClientDto.Email,
                    Telephone = newClientDto.Telephone
                };
                _context.Clients.Add(existingClient);
                await _context.SaveChangesAsync();
            }

            if (trip.DateFrom < DateTime.Now.Date)
                return BadRequest("Cannot sign up for a trip that has already started.");

            _context.Client_Trips.Add(new Client_Trip
            {
                IdTrip = tripId,
                IdClient = existingClient.IdClient,
                RegistrationDate = newClientDto.RegistrationDate,
                PaymentDate = newClientDto.PaymentDate
            });

            await _context.SaveChangesAsync();
            return StatusCode(201);
        }
    }
}
