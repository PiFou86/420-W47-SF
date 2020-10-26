using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace M01_DAL_Municipalite_MySQL
{
    public class MunicipaliteContextMySQL : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=municipalites;user=root;password=Passw0rd");
        }

        public DbSet<Municipalite> Municipalites { get; set; }
    }
}
