using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Models;

namespace src
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceDetailController : ControllerBase
    {
        private readonly PriceDetailDbContext _context;

        public PriceDetailController(PriceDetailDbContext context)
        {
            _context = context;
        }

        // GET: api/PriceDetail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PriceInfo>>> GetPriceInfo()
        {
          if (_context.PriceInfo == null)
          {
              return NotFound();
          }
            return await _context.PriceInfo.ToListAsync();
        }

        // GET: api/PriceDetail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PriceInfo>> GetPriceInfo(int id)
        {
          if (_context.PriceInfo == null)
          {
              return NotFound();
          }
            var priceInfo = await _context.PriceInfo.FindAsync(id);

            if (priceInfo == null)
            {
                return NotFound();
            }

            return priceInfo;
        }

        // PUT: api/PriceDetail/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPriceInfo(int id, PriceInfo priceInfo)
        {
            if (id != priceInfo.PriceValueId)
            {
                return BadRequest();
            }

            _context.Entry(priceInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PriceInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PriceDetail
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PriceInfo>> PostPriceInfo(PriceInfo priceInfo)
        {
          if (_context.PriceInfo == null)
          {
              return Problem("Entity set 'PriceDetailDbContext.PriceInfo'  is null.");
          }
            _context.PriceInfo.Add(priceInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPriceInfo", new { id = priceInfo.PriceValueId }, priceInfo);
        }

        // DELETE: api/PriceDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePriceInfo(int id)
        {
            if (_context.PriceInfo == null)
            {
                return NotFound();
            }
            var priceInfo = await _context.PriceInfo.FindAsync(id);
            if (priceInfo == null)
            {
                return NotFound();
            }

            _context.PriceInfo.Remove(priceInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PriceInfoExists(int id)
        {
            return (_context.PriceInfo?.Any(e => e.PriceValueId == id)).GetValueOrDefault();
        }
    }
}
