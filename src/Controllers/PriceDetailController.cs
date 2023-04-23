using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Models;

namespace src.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PriceDetailController : ControllerBase
    {
        private readonly PriceDetailDbContext _context;

        public PriceDetailController(PriceDetailDbContext context)
        {
            _context = context;
        }

        // GET: PriceDetail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PriceInfo>>> GetPriceInfo()
        {
            Console.WriteLine("PriceDetailController | GetPriceInfo()");
            if (!_context.PriceInfo.Any())
            {
                return NotFound();
            }

            return await _context.PriceInfo.ToListAsync();
        }

        // GET: PriceDetail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<PriceInfo>>> GetPriceInfo(string id)
        {
            Console.WriteLine("PriceDetailController | GetPriceInfo(id)");
            List<PriceInfo> priceInfos = await _context.PriceInfo.Where(pi => pi.CatalogEntryCode == id).ToListAsync();
            if (priceInfos.Count == 0)
            {
                return NotFound();
            }
            return priceInfos;

        }
        private bool PriceInfoExists(int id)
        {
            return (_context.PriceInfo?.Any(e => e.PriceValueId == id)).GetValueOrDefault();
        }
    }
}