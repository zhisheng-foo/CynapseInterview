using Microsoft.EntityFrameworkCore;
using MyBackend.Models;

namespace MyBackend.Data
{
    public class AllocationDbContext : DbContext
    {
        public AllocationDbContext(DbContextOptions<AllocationDbContext> options) : base(options) { }

        public DbSet<AllocationResult> Allocations { get; set; }
    }
}
