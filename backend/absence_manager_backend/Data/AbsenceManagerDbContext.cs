using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AbsenceManagerDbContext : DbContext
    {
        public AbsenceManagerDbContext(DbContextOptions<AbsenceManagerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
