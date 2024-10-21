using M01_Configuration_Application;
using M01_DAL_Municipalite_SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DSED_M01_Fichiers_Texte
{
    public class MunicipaliteContextFactory : IDesignTimeDbContextFactory<MunicipaliteContextSQLServer>
    {
        public MunicipaliteContextSQLServer CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MunicipaliteContextSQLServer>();
            optionsBuilder.UseSqlServer(Configuration.ChaineConnextion, b => b.MigrationsAssembly(this.GetType().Assembly.FullName));

            return new MunicipaliteContextSQLServer(optionsBuilder.Options);
        }
    }
}
