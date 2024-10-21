using Microsoft.EntityFrameworkCore;

namespace M01_DAL_Municipalite_MySQL
{
    public class MunicipaliteContextMySQL : DbContext
    {
        //private string m_chaineConnexion;

        public MunicipaliteContextMySQL(DbContextOptions<MunicipaliteContextMySQL> options)
    : base(options)
        {
            ;
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySQL(this.m_chaineConnexion);
        //}

        public DbSet<Municipalite> Municipalites { get; set; }
    }
}
