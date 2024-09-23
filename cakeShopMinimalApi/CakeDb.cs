using cakeShopMinimalApi.Model;
using Microsoft.EntityFrameworkCore;

namespace cakeShopMinimalApi
{
    class CakeDb : DbContext
    {
        public CakeDb(DbContextOptions options) : base(options) { }
        public DbSet<Cake> Cakes { get; set; } = null!;
    }
}
