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
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly MasterContext _context;

        public CountriesController(MasterContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetAll()
        {
            var countries = await _context.Countries
                .Select(c => new CountryDto
                {
                    IdCountry = c.IdCountry,
                    Name = c.Name
                })
                .ToListAsync();
            return Ok(countries);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CountryDto>> GetById(int id)
        {
            var c = await _context.Countries.FindAsync(id);
            if (c == null) return NotFound();

            var dto = new CountryDto
            {
                IdCountry = c.IdCountry,
                Name = c.Name
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CountryDto>> CreateCountry([FromBody] CountryDto newCountry)
        {
            if (await _context.Countries.AnyAsync(c => c.Name == newCountry.Name))
            {
                return BadRequest($"Country with name {newCountry.Name} already exists.");
            }

            var c = new Country
            {
                Name = newCountry.Name
            };
            _context.Countries.Add(c);
            await _context.SaveChangesAsync();

            newCountry.IdCountry = c.IdCountry;
            return CreatedAtAction(nameof(GetById), new { id = c.IdCountry }, newCountry);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryDto updated)
        {
            if (id != updated.IdCountry) return BadRequest("Id mismatch");

            var c = await _context.Countries.FindAsync(id);
            if (c == null) return NotFound();

            if (await _context.Countries.AnyAsync(x => x.Name == updated.Name && x.IdCountry != id))
            {
                return BadRequest($"Another country with name {updated.Name} already exists.");
            }

            c.Name = updated.Name;
            _context.Entry(c).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var c = await _context.Countries
                .Include(c => c.Country_Trips)
                .FirstOrDefaultAsync(c => c.IdCountry == id);

            if (c == null) return NotFound();

            if (c.Country_Trips.Any())
            {
                return BadRequest("Country is already assigned to at least one trip.");
            }

            _context.Countries.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
