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
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly MasterContext _context;

        public ClientsController(MasterContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClients()
        {
            var clients = await _context.Clients
                .Select(c => new ClientDto
                {
                    IdClient = c.IdClient,
                    Pesel = c.Pesel,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Telephone = c.Telephone
                })
                .ToListAsync();
            return Ok(clients);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClientDto>> GetClientById(int id)
        {
            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound();

            var dto = new ClientDto
            {
                IdClient = c.IdClient,
                Pesel = c.Pesel,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Telephone = c.Telephone
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] ClientDto newClient)
        {
            if (await _context.Clients.AnyAsync(c => c.Pesel == newClient.Pesel))
            {
                return BadRequest($"Client with PESEL {newClient.Pesel} already exists.");
            }

            var c = new Client
            {
                Pesel = newClient.Pesel,
                FirstName = newClient.FirstName,
                LastName = newClient.LastName,
                Email = newClient.Email,
                Telephone = newClient.Telephone
            };
            _context.Clients.Add(c);
            await _context.SaveChangesAsync();

            newClient.IdClient = c.IdClient;
            return CreatedAtAction(nameof(GetClientById), new { id = c.IdClient }, newClient);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientDto updated)
        {
            if (id != updated.IdClient) return BadRequest("Id mismatch");

            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound();

            if (await _context.Clients.AnyAsync(x => x.Pesel == updated.Pesel && x.IdClient != id))
            {
                return BadRequest($"Another client with PESEL {updated.Pesel} already exists.");
            }

            c.Pesel = updated.Pesel;
            c.FirstName = updated.FirstName;
            c.LastName = updated.LastName;
            c.Email = updated.Email;
            c.Telephone = updated.Telephone;

            _context.Entry(c).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var c = await _context.Clients
                .Include(c => c.Client_Trips)
                .FirstOrDefaultAsync(c => c.IdClient == id);

            if (c == null) return NotFound();

            if (c.Client_Trips.Any())
            {
                return BadRequest("Client is already assigned to at least one trip.");
            }

            _context.Clients.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
