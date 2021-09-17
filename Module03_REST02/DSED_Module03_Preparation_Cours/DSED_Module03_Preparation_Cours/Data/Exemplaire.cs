using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSED_Module03_Preparation_Cours.Data
{
    public class Exemplaire
    {
        public int ExemplaireId { get; set; }
        public bool EstEmprunte { get; set; }
        public int LivreId { get; set; }
    }
}
