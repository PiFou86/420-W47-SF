using Microsoft.EntityFrameworkCore;

namespace M01_DAL_Municipalite_SQLServer
{
    public class MunicipaliteContextSQLServer : DbContext//: IdentityDbContext
    {
        public MunicipaliteContextSQLServer(DbContextOptions options)
        : base(options)
        {
            ;
        }

        public DbSet<Municipalite> Municipalite { get; set; }
    }
}
