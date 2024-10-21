using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace M01_DAL_Municipalite_SQLServer
{
    public class MunicipaliteContextSQLServer : IdentityDbContext
    {
        public MunicipaliteContextSQLServer(DbContextOptions options)
        : base(options)
        {
            ;
        }

        public DbSet<Municipalite> Municipalite { get; set; }
    }
}
