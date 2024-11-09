namespace M01_Entite
{
    public interface IDepotMunicipalites
    {
        public Municipalite? ChercherMunicipaliteParCodeGeographique(int p_codeGeographique);
        public IEnumerable<Municipalite> ListerMunicipalitesActives();
        public void DesactiverMunicipalite(Municipalite p_municipalite);
        public void AjouterMunicipalite(Municipalite p_municipalite);
        public void MAJMunicipalite(Municipalite p_municipalite);
    }
}
