using Bulky.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) 
        {
            
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Action",
                    DiplayOrder = 1,
                }, new Category
                {
                Id = 2,
                    Name = "History",
                    DiplayOrder = 2,
                }, new Category
                {
                Id = 3,
                    Name = "abcd",
                    DiplayOrder = 3,
                }
                );
        }
    }
}
