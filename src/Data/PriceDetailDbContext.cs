using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using src.Models;

namespace src.Data
{
    public class PriceDetailDbContext : DbContext
    {
        public PriceDetailDbContext (DbContextOptions<PriceDetailDbContext> options)
            : base(options)
        {
        }

        public DbSet<src.Models.PriceInfo> PriceInfo { get; set; } = default!;
    }
}
