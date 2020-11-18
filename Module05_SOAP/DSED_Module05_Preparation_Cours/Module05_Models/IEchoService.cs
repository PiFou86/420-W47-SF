using System.ServiceModel;

namespace Module05_Models
{
    [ServiceContract]
    public interface IEchoService
    {
        [OperationContract]
        string Echo(string p_message);
        [OperationContract]
        decimal CalculInteretAnnuel(decimal p_montant, decimal p_taux);
    }
}