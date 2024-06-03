using InventoryHub.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.SQL
{
    public class InventoryHubDBContext(DbContextOptions<InventoryHubDBContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }
}
