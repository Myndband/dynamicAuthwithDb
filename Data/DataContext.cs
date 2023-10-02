

namespace DotnetAuthAndFileHandling.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder
        //        .UseSqlServer("Server=sql.bsite.net\\MSSQL2016;Database=usdotnetwebserver_erdb;User Id=dotnetwebserver_;Password=Password@123;Trusted_Connection=true;");
        //}

        public DbSet<User> Users => Set<User>();
        public DbSet<Customermodal> Customers { get; set; }
        //public DbSet<dbImage> Images { get; set; }
    }
}
