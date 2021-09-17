using M01_Configuration_Application;
using M01_DAL_Municipalite_MySQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DSED_M01_Fichiers_Texte
{
    public class MunicipaliteContextFactory : IDesignTimeDbContextFactory<MunicipaliteContextMySQL>
    {
        public MunicipaliteContextMySQL CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MunicipaliteContextMySQL>();
            optionsBuilder.UseMySQL(Configuration.ChaineConnextion, b => b.MigrationsAssembly(this.GetType().Assembly.FullName));

            return new MunicipaliteContextMySQL(optionsBuilder.Options);
        }
    }
}
