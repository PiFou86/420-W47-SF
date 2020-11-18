using Module05_Models;
using System;

namespace DSED_Module05_Preparation_Cours
{
    public class EchoService : IEchoService
    {
        public decimal CalculInteretAnnuel(decimal p_montant, decimal p_taux)
        {
            if (p_montant < 0.0m)
            {
                throw new ArgumentOutOfRangeException(nameof(p_montant), "Le montant ne doit pas être négatif");
            }

            if (p_taux < 0.0m || p_taux > 1.0m)
            {
                throw new ArgumentOutOfRangeException(nameof(p_taux), "Le taux doit être compris entre 0.0 et 1.0");
            }

            return p_montant * p_taux;
        }

        public string Echo(string p_message)
        {
            return p_message;
        }
    }
}