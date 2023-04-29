using System.Globalization;
using Microsoft.EntityFrameworkCore;
using src.Models;

namespace src.Data;

public static class SeedData
{
    private static void LoadDataFromCsv(DbContext context)
    {
        var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser("data/price_detail.csv");
        parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
        parser.SetDelimiters("\t");
        
        /*Skip heading*/
        parser.ReadFields();

        while (!parser.EndOfData)
        {
            string?[]? fields = parser.ReadFields();
            
            decimal unitPrice;
            if (decimal.TryParse(fields?[8] ?? string.Empty, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal tempUnitPrice))
            {
                unitPrice = tempUnitPrice;
            }
            else
            {
                throw new FormatException($"Unable to parse '{fields?[8]}' as a decimal.");
            }
            
            var priceInfo = new PriceDetail
            {
                PriceValueId = int.Parse(fields?[0] ?? string.Empty),
                Created = DateTime.Parse(fields?[1] ?? string.Empty),
                Modified = DateTime.Parse(fields?[2] ?? string.Empty),
                CatalogEntryCode = fields?[3],
                MarketId = fields?[4],
                CurrencyCode = fields?[5],
                ValidFrom = DateTime.Parse(fields?[6] ?? string.Empty),
                ValidUntil = fields?[7] == "NULL" ? null : DateTime.Parse(fields?[7] ?? string.Empty),
                UnitPrice = unitPrice
            };
            context.Add(priceInfo);
        }
        context.Database.OpenConnection();
        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT PriceInfo ON");
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT PriceInfo OFF");
        context.Database.CloseConnection();
        Console.WriteLine("LoadDataFromCsv() done.");
    }

    public static void Initialize(IServiceProvider services)
    {
        using var context = new PriceDetailDbContext(
            services.GetRequiredService<DbContextOptions<PriceDetailDbContext>>());
        if (!context.PriceInfo.Any())
        {
            LoadDataFromCsv(context);
        }

    }
}