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
            var priceInfos = await _context.PriceInfo.Where(pi => pi.CatalogEntryCode == id).ToListAsync();
            if (priceInfos.Count == 0)
            {
                return NotFound();
            }

            return OptimizePriceInfos(priceInfos);
            ;
        }

        private List<PriceInfo> OptimizePriceInfos(List<PriceInfo> priceInfos)
        {
            /*
            Det kan forekomma flera priser i samma valuta och pa samma marknad som ar giltiga under samma period. Då ska det billigaste priset gälla.

                Exempeldata
                Ett exempel med SKU 27773-02
            I datasetet (CSV-filen) finns följande priser i SEK för den svenska marknaden

            Exempeldata-tabell:
                | PriceValueId	| Created | Modified | CatalogEntryCode | MarketId | CurrencyCode | ValidFrom | ValidUntil | UnitPrice |
                | - | - | - | - | - | - | - | - | - | 
                | 169530679	| 2018-08-07 00:00:00 | 2018-08-07 00:00:00 | 27773-02 | sv | SEK | 2018-08-07 00:00:00 | 2018-08-19 00:00:00 | 326.800000000 |
                | 169530677 | 2018-08-07 00:00:00 | 2018-08-07 00:00:00 | 27773-02 | sv | SEK | 2018-06-18 00:00:00 | 2018-08-05 00:00:00 | 399.600000000 |
                | 169530676 | 2018-08-07 00:00:00 | 2018-08-07 00:00:00 | 27773-02 | sv | SEK | 1970-01-01 00:00:00 | NULL | 439.600000000 |
                | 169530678 | 2018-08-07 00:00:00 | 2018-08-07 00:00:00 | 27773-02 | sv | SEK | 2018-08-01 00:00:00 | 2018-08-05 00:00:00 | 326.800000000 |
 
                Output:
            Exemplet med produkt 27773-02 ska i din lösning generera följande optimerade tabell med aktuellt försäljningspris.
                Output-tabell GT Haptik Thin:
            Marknad	Pris	Valuta	Start och slut
            sv	439,60	SEK	1970-01-01 00:00 – 2018-06-18 00:00
            sv	399,60	SEK	2018-06-18 00:00 – 2018-08-01 00:00
            sv	326,80	SEK	2018-08-01 00:00 – 2018-08-05 00:00
            sv	439,60	SEK	2018-08-05 00:00 – 2018-08-07 00:00
            sv	326,80	SEK	2018-08-07 00:00 – 2018-08-19 00:00
            sv	439,60	SEK	2018-08-19 00:00 –

            Notera att det i putput-tabellen ovan enbart listas ett giltigt pris för varje tidpunkt per marknad och per valuta (även fast det i exempeldatan är överlappande tider). Tänk på att det kan finnas edgecase som inte täcks av exemplet ovan.

                Output-tabellen ovan med SKU 27773-02 kan användas för att testa din lösning. Om din lösning visar annan data än den i output-tabellen för denna produkt i marknad 'sv' är något fel.*/
            
            var nonOverlappingPrices = new List<PriceInfo>();
            PriceInfo prevPrice = null;

            foreach (var price in priceInfos)
            {
                if (prevPrice != null &&
                    (prevPrice.ValidUntil == null || prevPrice.ValidUntil.Value >= price.ValidFrom))
                {
                    if (prevPrice.UnitPrice < price.UnitPrice)
                    {
                        prevPrice.ValidUntil = price.ValidFrom.AddDays(-1);
                        nonOverlappingPrices.Add(prevPrice);
                        nonOverlappingPrices.Add(price);
                    }
                    else
                    {
                        var tempPrice = new PriceInfo
                        {
                            PriceValueId = prevPrice.PriceValueId,
                            Created = prevPrice.Created,
                            Modified = prevPrice.Modified,
                            CatalogEntryCode = prevPrice.CatalogEntryCode,
                            MarketId = prevPrice.MarketId,
                            CurrencyCode = prevPrice.CurrencyCode,
                            ValidFrom = prevPrice.ValidFrom,
                            ValidUntil = price.ValidFrom.AddDays(-1),
                            UnitPrice = prevPrice.UnitPrice,
                        };

                        nonOverlappingPrices.Add(tempPrice);

                        price.ValidFrom = prevPrice.ValidUntil.HasValue
                            ? prevPrice.ValidUntil.Value.AddDays(1)
                            : price.ValidFrom;
                        nonOverlappingPrices.Add(price);

                        prevPrice.ValidFrom = price.ValidUntil.HasValue
                            ? price.ValidUntil.Value.AddDays(1)
                            : prevPrice.ValidFrom;
                    }
                }
                else
                {
                    nonOverlappingPrices.Add(price);
                }

                prevPrice = price;
            }


            /*
            var groupedByMarket = priceInfos.GroupBy(pi => pi.MarketId);
            var nonOverlappingPrices = new List<PriceInfo>();
            
            foreach(var marketGroup in groupedByMarket)
            {
                var marketId = marketGroup.Key;
                var marketPrices = marketGroup.ToList();
                var groupedByCurrency = marketPrices.GroupBy(pi => pi.CurrencyCode);
                foreach(var currencyGroup in groupedByCurrency)
                {
                    var currencyCode = currencyGroup.Key;
                    var currencyPrices = currencyGroup.ToList();
                    var groupedByValidFrom = currencyPrices.GroupBy(pi => pi.ValidFrom);
                    foreach(var validFromGroup in groupedByValidFrom)
                    {
                        var validFrom = validFromGroup.Key;
                        var validFromPrices = validFromGroup.ToList();
                        var groupedByValidUntil = validFromPrices.GroupBy(pi => pi.ValidUntil);
                        foreach(var validUntilGroup in groupedByValidUntil)
                        {
                            var validUntil = validUntilGroup.Key;
                            var validUntilPrices = validUntilGroup.ToList();
                            var cheapestPrice = validUntilPrices.OrderBy(pi => pi.UnitPrice).First();
                            nonOverlappingPrices.Add(cheapestPrice);
                        }
                    }
                }
            }*/

            /*
            /*Sort PriceInfos in each MarketId list by UnitPrice#1#
            foreach (var (_, value) in priceInfoMap)
            {
                value.Sort((x, y) => x.UnitPrice.CompareTo(y.UnitPrice));
            }

            foreach (string? key in from key in priceInfoMap.Keys from priceInfo in priceInfoMap[key] select key)
            {
                
            }

            /*Print map#1#
            foreach (var (key, value) in priceInfoMap)
            {
                Console.WriteLine($"MarketId: {key}");
                foreach (var priceInfo in value)
                {
                    Console.WriteLine($"\t{priceInfo.UnitPrice}");
                }
            }
            
            var priceInfoList = new List<PriceInfo>();
            /*Convert map to array of PrinceInfo#1#
            foreach (var key in priceInfoMap.Keys)
            {
                priceInfoList.AddRange(priceInfoMap[key]);
            }
            */

            return nonOverlappingPrices;
        }

        private bool PriceInfoExists(int id)
        {
            return (_context.PriceInfo?.Any(e => e.PriceValueId == id)).GetValueOrDefault();
        }
    }
}