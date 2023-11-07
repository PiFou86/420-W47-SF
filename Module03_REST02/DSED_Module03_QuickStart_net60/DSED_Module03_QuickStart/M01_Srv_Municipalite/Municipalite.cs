using System;
using System.Collections.Generic;

namespace M01_Srv_Municipalite
{
    public class Municipalite
    {
        public int CodeGeographique { get; private set; }
        public string NomMunicipalite { get; private set; }
        public string AdresseCourriel { get; private set; }
        public string AdresseWeb { get; private set; }
        public DateTime DateProchaineElection { get; private set; }
        public bool Actif { get; private set; }

        public Municipalite(int p_codeGeographique, string p_nomMunicipalite, string p_adresseCourriel, string p_adresseWeb, DateTime p_dateProchaineElection, bool p_actif)
        {
            this.CodeGeographique = p_codeGeographique;
            this.NomMunicipalite = p_nomMunicipalite;
            this.AdresseCourriel = p_adresseCourriel;
            this.AdresseWeb = p_adresseWeb;
            this.DateProchaineElection = p_dateProchaineElection;
            this.Actif = p_actif;
        }

        public override bool Equals(object obj)
        {
            Municipalite objAComparer = obj as Municipalite;

            return objAComparer != null
                && this.CodeGeographique == objAComparer.CodeGeographique
                && string.Compare(this.NomMunicipalite, objAComparer.NomMunicipalite) == 0
                && string.Compare(this.AdresseCourriel, objAComparer.AdresseCourriel) == 0
                && string.Compare(this.AdresseWeb, objAComparer.AdresseWeb) == 0
                && this.DateProchaineElection == objAComparer.DateProchaineElection
                && this.Actif == objAComparer.Actif;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CodeGeographique, NomMunicipalite, AdresseCourriel, AdresseWeb, DateProchaineElection, Actif);
        }

        public static bool operator ==(Municipalite left, Municipalite right)
        {
            return EqualityComparer<Municipalite>.Default.Equals(left, right);
        }

        public static bool operator !=(Municipalite left, Municipalite right)
        {
            return !(left == right);
        }
    }
}