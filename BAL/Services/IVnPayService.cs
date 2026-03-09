using DAL.Models;

namespace BAL.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Order order, string ipAddress);
        (bool IsValid, string VnpResponseCode, string VnpTransactionNo, string TxnRef) ValidateCallback(IDictionary<string, string> queryParams);
    }
}
