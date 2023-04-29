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

        // GET: PriceInfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PriceDetail>>> GetPriceDetail()
        {
            Console.WriteLine("PriceDetailController | GetPriceDetail()");
            if (!_context.PriceInfo.Any())
            {
                return NotFound();
            }

            return await _context.PriceInfo.ToListAsync();
        }

        // GET: PriceInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<PriceDetail>>> GetPriceDetail(string id)
        {
            Console.WriteLine("PriceDetailController | GetPriceDetail(id)");
            var priceInfos = await _context.PriceInfo.Where(pi => pi.CatalogEntryCode == id).ToListAsync();
            if (priceInfos.Count == 0)
            {
                return NotFound();
            }

            return OptimizePriceDetails(priceInfos);
            ;
        }

        private List<PriceDetail> OptimizePriceDetails(List<PriceDetail> priceDetails)
        {
            // Variable to group the pricinfos based of market.    
            var priceDetailDictionary = new Dictionary<string, List<PriceDetail>>();

            // Fill dictionary with priceDetails where key is priceDetail.MarketId and priceDetail.CurrencyCode and value is a list of priceDetails
            foreach (var priceDetail in priceDetails)
            {
                if (priceDetail.MarketId != null &&
                    priceDetailDictionary.TryGetValue(priceDetail.MarketId + '-' + priceDetail.CurrencyCode,
                        out var info))
                {
                    info.Add(priceDetail);
                }
                else
                {
                    priceDetailDictionary.Add(priceDetail.MarketId + '-' + priceDetail.CurrencyCode,
                        new List<PriceDetail>() { priceDetail });
                }
            }

            /*foreach (var priceDetail in priceDetails)
            {
                if (priceDetail.MarketId != null &&
                    priceDetailDictionary.TryGetValue(priceDetail.MarketId, out var info))
                {
                    info.Add(priceDetail);
                }
                else
                {
                    priceDetailDictionary.Add(priceDetail.MarketId!, new List<PriceDetail>() { priceDetail });
                }
            }*/

            // Variable to hold the priceinfos that are decided.
            var decidedForPriceDetails = new Dictionary<string, List<PriceDetail>>();

            // Loop through all markets
            foreach (var key in priceDetailDictionary.Keys)
            {
                var marketId = key.Split('-')[0];
                var currencyCode = key.Split('-')[1];

                // Add the market to the setMarketPriceInfos dictionary
                decidedForPriceDetails.Add(key, new List<PriceDetail>());

                // Sort the priceinfos in the market by ValidFrom and then by UnitPrice, highest first.
                priceDetailDictionary[key] = priceDetailDictionary[key].OrderBy(pi => pi.ValidFrom)
                    .ThenByDescending(pi => pi.UnitPrice).ToList();

                // Loop through all priceinfos in the market
                foreach (var currentPriceDetail in priceDetailDictionary[key])
                {
                    // If the currentPriceInfo's ValidUntil is null, set it to DateTime.MaxValue
                    currentPriceDetail.ValidUntil ??= DateTime.MaxValue;

                    // DOES NOT OVERLAP AT ALL
                    if (decidedForPriceDetails[key].All(pi =>
                            pi.ValidFrom > currentPriceDetail.ValidUntil ||
                            pi.ValidUntil < currentPriceDetail.ValidFrom &&
                            pi.UnitPrice > currentPriceDetail.UnitPrice))
                    {
                        if (marketId == "ko") Console.WriteLine("Adding priceinfo that is not overlapping");
                        decidedForPriceDetails[key].Add(currentPriceDetail);
                    }

                    // OVERLAPS ENTIRELY
                    else if (decidedForPriceDetails[key].Any(pi =>
                                 currentPriceDetail.ValidFrom <= pi.ValidFrom &&
                                 currentPriceDetail.ValidUntil >= pi.ValidUntil &&
                                 pi.UnitPrice > currentPriceDetail.UnitPrice))
                    {
                        if (marketId == "ko") Console.WriteLine("Adding priceinfo that is overlapping entirely");
                        decidedForPriceDetails[key].RemoveAll(pi =>
                            currentPriceDetail.ValidFrom <= pi.ValidFrom &&
                            currentPriceDetail.ValidUntil >= pi.ValidUntil &&
                            pi.UnitPrice > currentPriceDetail.UnitPrice);
                        decidedForPriceDetails[key].Add(currentPriceDetail);
                    }

                    // OVERLAPS PARTIALLY FROM THE LEFT
                    else if (decidedForPriceDetails[key].Any(pi =>
                                 currentPriceDetail.ValidFrom <= pi.ValidFrom &&
                                 currentPriceDetail.ValidUntil > pi.ValidFrom &&
                                 pi.UnitPrice > currentPriceDetail.UnitPrice))
                    {
                        if (marketId == "ko")
                            Console.WriteLine("Adding priceinfo that is overlapping partially from the left");

                        // Find the priceInfo that overlaps partially from the left
                        var priceDetail = decidedForPriceDetails[key].First(pi =>
                            currentPriceDetail.ValidFrom <= pi.ValidFrom &&
                            currentPriceDetail.ValidUntil > pi.ValidFrom &&
                            pi.UnitPrice > currentPriceDetail.UnitPrice);

                        // Remove the priceInfo from the setMarketPriceInfos
                        decidedForPriceDetails[key].Remove(priceDetail);

                        // Add the priceInfo to the setMarketPriceInfos with its validFrom set to the currentPriceInfo's validUntil
                        decidedForPriceDetails[key].Add(new PriceDetail()
                        {
                            PriceValueId = priceDetail.PriceValueId,
                            Created = priceDetail.Created,
                            Modified = priceDetail.Modified,
                            CatalogEntryCode = priceDetail.CatalogEntryCode,
                            MarketId = priceDetail.MarketId,
                            CurrencyCode = priceDetail.CurrencyCode,
                            ValidFrom = currentPriceDetail.ValidUntil,
                            ValidUntil = priceDetail.ValidUntil,
                            UnitPrice = priceDetail.UnitPrice
                        });

                        // Add the currentPriceInfo to the setMarketPriceInfos
                        decidedForPriceDetails[key].Add(currentPriceDetail);
                    }

                    // OVERLAPS PARTIALLY FROM THE RIGHT
                    else if (decidedForPriceDetails[key].Any(pd =>
                                 currentPriceDetail.ValidFrom < pd.ValidUntil &&
                                 currentPriceDetail.ValidUntil >= pd.ValidUntil &&
                                 pd.UnitPrice > currentPriceDetail.UnitPrice))
                    {
                        if (marketId == "ko")
                            Console.WriteLine("Adding priceinfo that is overlapping partially from the right");

                        // Find the priceInfo that overlaps partially from the right
                        var priceDetail = decidedForPriceDetails[key].First(pd =>
                            currentPriceDetail.ValidFrom < pd.ValidUntil &&
                            currentPriceDetail.ValidUntil >= pd.ValidUntil &&
                            pd.UnitPrice > currentPriceDetail.UnitPrice);

                        // Remove the priceInfo from the setMarketPriceInfos
                        decidedForPriceDetails[key].Remove(priceDetail);

                        // Add the priceInfo to the setMarketPriceInfos with its validUntil set to the currentPriceInfo's validFrom
                        decidedForPriceDetails[key].Add(new PriceDetail()
                        {
                            PriceValueId = priceDetail.PriceValueId,
                            Created = priceDetail.Created,
                            Modified = priceDetail.Modified,
                            CatalogEntryCode = priceDetail.CatalogEntryCode,
                            MarketId = priceDetail.MarketId,
                            UnitPrice = priceDetail.UnitPrice,
                            CurrencyCode = priceDetail.CurrencyCode,
                            ValidFrom = priceDetail.ValidFrom,
                            ValidUntil = currentPriceDetail.ValidFrom
                        });

                        // Add the currentPriceInfo to the setMarketPriceInfos
                        decidedForPriceDetails[key].Add(currentPriceDetail);
                    }

                    // UNDERLAPS ENTIRELY
                    else if (decidedForPriceDetails[key].Any(pd =>
                                 currentPriceDetail.ValidFrom > pd.ValidFrom &&
                                 currentPriceDetail.ValidUntil < pd.ValidUntil &&
                                 pd.UnitPrice > currentPriceDetail.UnitPrice))
                    {
                        if (marketId == "ko") Console.WriteLine("Adding priceinfo that is underlapping entirely");

                        // Find the priceInfo that overlaps entirely
                        var priceDetail = decidedForPriceDetails[key].First(pd =>
                            currentPriceDetail.ValidFrom > pd.ValidFrom &&
                            currentPriceDetail.ValidUntil < pd.ValidUntil);

                        // Remove the priceInfo from the setMarketPriceInfos
                        decidedForPriceDetails[key].Remove(priceDetail);

                        // Add the priceInfo to the setMarketPriceInfos with its validTo set to the currentPriceInfo's validFrom
                        decidedForPriceDetails[key].Add(new PriceDetail()
                        {
                            PriceValueId = priceDetail.PriceValueId,
                            Created = priceDetail.Created,
                            Modified = priceDetail.Modified,
                            CatalogEntryCode = priceDetail.CatalogEntryCode,
                            MarketId = priceDetail.MarketId,
                            UnitPrice = priceDetail.UnitPrice,
                            CurrencyCode = priceDetail.CurrencyCode,
                            ValidFrom = priceDetail.ValidFrom,
                            ValidUntil = currentPriceDetail.ValidFrom
                        });

                        // Add the currentPriceInfo to the setMarketPriceInfos
                        decidedForPriceDetails[key].Add(currentPriceDetail);

                        // Add the priceInfo to the setMarketPriceInfos with its validFrom set to the currentPriceInfo's validUntil
                        decidedForPriceDetails[key].Add(new PriceDetail()
                        {
                            PriceValueId = priceDetail.PriceValueId,
                            Created = priceDetail.Created,
                            Modified = priceDetail.Modified,
                            CatalogEntryCode = priceDetail.CatalogEntryCode,
                            MarketId = priceDetail.MarketId,
                            UnitPrice = priceDetail.UnitPrice,
                            CurrencyCode = priceDetail.CurrencyCode,
                            ValidFrom = currentPriceDetail.ValidUntil,
                            ValidUntil = priceDetail.ValidUntil
                        });
                    }

                    if (marketId == "ko")
                    {
                        decidedForPriceDetails[key].ForEach(pd =>
                            Console.WriteLine(
                                $"PriceInfo: {pd.PriceValueId} - {pd.ValidFrom} - {pd.ValidUntil} - {pd.UnitPrice}"));
                    }

                    ;
                }
            }

            // Set the setMarketPriceInfos as a list of priceinfos. Order by marketId, then by currencyCode and then by validFrom. 
            priceDetails = decidedForPriceDetails.SelectMany(kvp => kvp.Value).OrderBy(pd => pd.MarketId)
                .ThenBy(pd => pd.CurrencyCode).ThenBy(pd => pd.ValidFrom).ToList();

            // Set the validUntil of priceinfos with DateTime.MaxValue to null
            foreach (var priceDetail in priceDetails.Where(priceDetail => priceDetail.ValidUntil == DateTime.MaxValue))
            {
                priceDetail.ValidUntil = null;
            }

            return priceDetails;
        }
    }
}