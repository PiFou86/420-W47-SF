using MyNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSED_Module03_Preparation_Cours_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            LivresClient lc = new LivresClient();
            // PFL : pas obligatoire ici. L'adresse devrait venir de la configuration appsettings.json
            lc.BaseUrl = "https://localhost:5001";
            Task<ICollection<Livre>> getTask = lc.GetAllAsync();
            getTask.Wait();
            List<Livre> livres = getTask.Result.ToList();

            livres.ForEach(livre => Console.Out.WriteLine(livre.Titre));
        }
    }
}
