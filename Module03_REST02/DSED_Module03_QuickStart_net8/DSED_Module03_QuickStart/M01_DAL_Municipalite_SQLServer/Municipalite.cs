using System;
using srvm = M01_Srv_Municipalite;

namespace M01_DAL_Municipalite_SQLServer
{
    // DTO
    public class Municipalite
    {
        public int MunicipaliteId { get; set; }
        public string NomMunicipalite { get; set; }
        public string AdresseCourriel { get; set; }
        public string AdresseWeb { get; set; }
        public DateTime DateProchaineElection { get; set; }
        public bool Actif { get; set; }

        public Municipalite() { ; }

        public Municipalite(srvm.Municipalite p_municipalite)
        {
            this.MunicipaliteId = p_municipalite.CodeGeographique;
            this.NomMunicipalite = p_municipalite.NomMunicipalite;
            this.AdresseCourriel = p_municipalite.AdresseCourriel;
            this.AdresseWeb = p_municipalite.AdresseWeb;
            this.DateProchaineElection = p_municipalite.DateProchaineElection;
            this.Actif = p_municipalite.Actif;
        }

        public srvm.Municipalite VersEntite()
        {
            return new srvm.Municipalite(
                this.MunicipaliteId,
                this.NomMunicipalite,
                this.AdresseCourriel,
                this.AdresseWeb,
                this.DateProchaineElection,
                this.Actif
            );
        }
    }
}