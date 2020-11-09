using System.ComponentModel.DataAnnotations;

namespace DSED_Module03_Preparation_Cours.Models
{
    public class Livre
    {
        public int LivreId { get; set; }
        [Required]
        public string Titre { get; set; }
    }
}