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
            Console.WriteLine("Hello World!");
            LivresClient lc = new LivresClient();
            Task<ICollection<Livre>> getTask = lc.GetAllAsync();
            getTask.Wait();
            List<Livre> livres = getTask.Result.ToList();
        }
    }
}
