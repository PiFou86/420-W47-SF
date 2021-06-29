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
        private static Dictionary<string, string> _connexionNomTableau = new Dictionary<string, string>();
        private static Dictionary<string, List<Ligne>> _dessin = new Dictionary<string, List<Ligne>>();

        public async Task CreerRejoindre(string p_nomTableau)
        {
            string connexionId = this.Context.ConnectionId;
            if (!_connexionNomTableau.ContainsKey(connexionId))
            {
                _connexionNomTableau.Add(connexionId, null);
            }

            if (!string.IsNullOrWhiteSpace(_connexionNomTableau[connexionId]))
            {
                await Groups.RemoveFromGroupAsync(connexionId, _connexionNomTableau[connexionId]);
                _connexionNomTableau[connexionId] = null;
            }

            if (!_dessin.ContainsKey(p_nomTableau))
            {
                _dessin.Add(p_nomTableau, new List<Ligne>());
                await Clients.All.SendAsync("MAJTableauxDisponibles", _dessin.Keys);
            }

            await Groups.AddToGroupAsync(connexionId, p_nomTableau);
            _connexionNomTableau[connexionId] = p_nomTableau;
            await Clients.Caller.SendAsync("DemarrageTableau", _dessin[p_nomTableau]);
        }

        public async Task DessinerLigne(Ligne p_ligne)
        {
            string connexionId = this.Context.ConnectionId;

            if (_connexionNomTableau.ContainsKey(connexionId))
            {
                string nomTableau = _connexionNomTableau[connexionId];
                _dessin[nomTableau].Add(p_ligne);

                await Clients.Group(nomTableau).SendAsync("DessinerLigne", p_ligne);
            }
        }

        public async Task EffacerTableau()
        {
            string connexionId = this.Context.ConnectionId;

            if (_connexionNomTableau.ContainsKey(connexionId))
            {
                string nomTableau = _connexionNomTableau[connexionId];
                _dessin[nomTableau].Clear();

                await Clients.Group(nomTableau).SendAsync("EffacerTableau");
            }
        }

        public async override Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("MAJTableauxDisponibles", _dessin.Keys);

            await base.OnConnectedAsync();
        }

    }
}
