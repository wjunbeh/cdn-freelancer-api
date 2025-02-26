using Microsoft.EntityFrameworkCore;
using CdnFreelancerApi.Models;

namespace CdnFreelancerApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}