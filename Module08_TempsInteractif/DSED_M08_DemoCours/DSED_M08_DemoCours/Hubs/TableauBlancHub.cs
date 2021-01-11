using DSED_M08_DemoCours.Entite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace DSED_M08_DemoCours.Hubs
{
    public class TableauBlancHub : Hub
    {
        public static List<Ligne> _dessin = new List<Ligne>();

        public async Task DessinerLigne(Ligne ligne)
        {
            _dessin.Add(ligne);

            await Clients.All.SendAsync("DessinerLigne", ligne);
        }

        public async Task EffacerTableau()
        {
            _dessin.Clear();

            await Clients.All.SendAsync("EffacerTableau");
        }

        public async override Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("DemarrageTableau", _dessin);

            await base.OnConnectedAsync();
        }

    }
}
