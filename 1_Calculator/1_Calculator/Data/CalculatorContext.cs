using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

namespace _1_Calculator.Data
{
    public class CalculatorContext: DbContext
    {
        public DbSet<DataInputVariant> DataInputVariants { get; set; }
        public CalculatorContext(DbContextOptions<CalculatorContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
        }


    }
}
