using System;
using System.Collections.Generic;
using System.Linq;
using M01_Srv_Municipalite;

namespace M01_DAL_Municipalite_SQLServer
{
    public class DepotMunicipalitesSQLServer : IDepotMunicipalites
    {
        private MunicipaliteContextSQLServer m_contexte;

        public DepotMunicipalitesSQLServer(MunicipaliteContextSQLServer p_contexte)
        {
            if (p_contexte is null)
            {
                throw new ArgumentNullException(nameof(p_contexte));
            }

            this.m_contexte = p_contexte;
        }

        public void AjouterMunicipalite(M01_Srv_Municipalite.Municipalite p_municipalite)
        {
            if (p_municipalite is null)
            {
                throw new ArgumentNullException(nameof(p_municipalite));
            }

            if (this.m_contexte.Municipalite.Any(m => m.MunicipaliteId == p_municipalite.CodeGeographique))
            {
                throw new InvalidOperationException($"La municipalité d'identifiant {p_municipalite.CodeGeographique} existe déjà dans le dépôt de données.");
            }

            this.m_contexte.Municipalite.Add(new Municipalite(p_municipalite));
            this.m_contexte.SaveChanges();
        }

        public M01_Srv_Municipalite.Municipalite ChercherMunicipaliteParCodeGeographique(int p_codeGeographique)
        {
            return this.m_contexte.Municipalite.Where(m => m.MunicipaliteId == p_codeGeographique).Select(m => m.VersEntite()).SingleOrDefault();
        }

        public void DesactiverMunicipalite(M01_Srv_Municipalite.Municipalite p_municipalite)
        {
            if (p_municipalite is null)
            {
                throw new ArgumentNullException(nameof(p_municipalite));
            }

            Municipalite m = this.m_contexte.Municipalite.Where(m => m.MunicipaliteId == p_municipalite.CodeGeographique).SingleOrDefault();
            if (m is null)
            {
                throw new InvalidOperationException($"La municipalité d'identifiant {p_municipalite.CodeGeographique} n'existe pas dans le dépôt de données.");
            }

            m.Actif = false;
            this.m_contexte.Municipalite.Update(m);
            this.m_contexte.SaveChanges();
        }

        public IEnumerable<M01_Srv_Municipalite.Municipalite> ListerMunicipalitesActives()
        {
            return this.m_contexte.Municipalite.Where(m => m.Actif).Select(m => m.VersEntite()).ToList();
        }

        public void MAJMunicipalite(M01_Srv_Municipalite.Municipalite p_municipalite)
        {
            if (p_municipalite is null)
            {
                throw new ArgumentNullException(nameof(p_municipalite));
            }

            if (!this.m_contexte.Municipalite.Any(m => m.MunicipaliteId == p_municipalite.CodeGeographique))
            {
                throw new InvalidOperationException($"La municipalité d'identifiant {p_municipalite.CodeGeographique} n'existe pas dans le dépôt de données.");
            }

            this.m_contexte.Municipalite.Update(new Municipalite(p_municipalite));
            this.m_contexte.SaveChanges();
        }
    }
}
