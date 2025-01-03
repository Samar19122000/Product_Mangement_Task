using Microsoft.EntityFrameworkCore;
using task.Models;

namespace task
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
    }
}
