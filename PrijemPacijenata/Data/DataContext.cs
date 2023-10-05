using Microsoft.EntityFrameworkCore;


namespace PrijemPacijenata.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options) { }

        public DbSet<Pacijent> Pacijenti { get; set; }
        public DbSet<Doktor> Doktori { get; set; }
        public DbSet<Dijagnoza> Dijagnoze { get; set; }
    }
}
