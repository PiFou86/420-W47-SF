using System;

using SRVM = M01_Entite;

namespace M01_DAL_Municipalite_SQLServer
{
    // DTO
    public class Municipalite
    {
        public int MunicipaliteId { get; set; } = 0;
        public string NomMunicipalite { get; set; } = string.Empty;
        public string? AdresseCourriel { get; set; }
        public string? AdresseWeb { get; set; }
        public DateOnly? DateProchaineElection { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public bool Actif { get; set; }

        public Municipalite() { ; }

        public Municipalite(SRVM.Municipalite p_municipalite)
        {
            this.MunicipaliteId = p_municipalite.CodeGeographique;
            this.NomMunicipalite = p_municipalite.NomMunicipalite;
            this.AdresseCourriel = p_municipalite.AdresseCourriel;
            this.AdresseWeb = p_municipalite.AdresseWeb;
            this.DateProchaineElection = p_municipalite.DateProchaineElection;
            this.Actif = true;
        }

        public SRVM.Municipalite VersEntite()
        {
            return new SRVM.Municipalite(
                this.MunicipaliteId,
                this.NomMunicipalite,
                this.AdresseCourriel,
                this.AdresseWeb,
                this.DateProchaineElection
            );
        }
    }
}