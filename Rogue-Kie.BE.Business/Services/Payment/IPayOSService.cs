using Rogue_Kie.BE.Contracts.Payment;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.Payment
{
    public interface IPayOSService
    {
        Task<PaymentResponse> CreatePaymentLinkAsync(int userId, CreatePaymentLinkRequest request);
        Task<bool> ProcessWebhookAsync(PayOSWebhookRequest webhook);
        Task<PaymentResponse?> GetPaymentStatusAsync(long orderCode);
        Task<bool> CancelPaymentAsync(long orderCode);
        Task<bool> SimulateSuccessAsync(long orderCode);
    }
}
