using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.Payment;
using Rogue_Kie.BE.Contracts.Payment;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;

        public PaymentController(IPayOSService payOSService)
        {
            _payOSService = payOSService;
        }

        /// <summary>
        /// Tạo link thanh toán PayOS & Sinh mã VietQR cho người chơi nạp Gem/Coin/Item
        /// </summary>
        [HttpPost("create-payment-link")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin người dùng hợp lệ.");
            }

            var response = await _payOSService.CreatePaymentLinkAsync(userId, request);
            return Ok(response);
        }

        /// <summary>
        /// Webhook callback tự động từ PayOS khi chuyển khoản ngân hàng thành công
        /// </summary>
        [HttpPost("payos-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSWebhookRequest webhook)
        {
            var success = await _payOSService.ProcessWebhookAsync(webhook);
            if (!success)
            {
                return BadRequest(new { code = "01", desc = "Xử lý Webhook thất bại" });
            }

            return Ok(new { code = "00", desc = "Thành công" });
        }

        /// <summary>
        /// Kiểm tra trạng thái đơn hàng (Polling từ Client)
        /// </summary>
        [HttpGet("check-status/{orderCode}")]
        [Authorize]
        public async Task<IActionResult> CheckStatus(long orderCode)
        {
            var result = await _payOSService.GetPaymentStatusAsync(orderCode);
            if (result == null) return NotFound("Không tìm thấy giao dịch.");
            return Ok(result);
        }

        /// <summary>
        /// Hủy đơn hàng nạp tiền chưa thanh toán
        /// </summary>
        [HttpPost("cancel/{orderCode}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(long orderCode)
        {
            var success = await _payOSService.CancelPaymentAsync(orderCode);
            if (!success) return BadRequest("Không thể hủy giao dịch.");
            return Ok(new { message = "Hủy giao dịch thành công." });
        }

        /// <summary>
        /// Hủy TOÀN BỘ đơn hàng chưa thanh toán (PENDING) trên PayOS Server và DB
        /// </summary>
        [HttpPost("cancel-all-pending")]
        [Authorize]
        public async Task<IActionResult> CancelAllPendingPayments()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;
            if (!string.IsNullOrEmpty(userIdStr)) int.TryParse(userIdStr, out userId);

            int count = await _payOSService.CancelAllPendingPaymentsAsync(userId);
            return Ok(new { message = $"Đã hủy thành công {count} đơn hàng đang PENDING trên PayOS Server!" });
        }

        /// <summary>
        /// Giả lập thanh toán thành công trong môi trường Dev (không cần tốn tiền thật)
        /// </summary>
        [HttpPost("dev-simulate-success/{orderCode}")]
        [Authorize]
        public async Task<IActionResult> DevSimulateSuccess(long orderCode)
        {
            var success = await _payOSService.SimulateSuccessAsync(orderCode);
            if (!success) return BadRequest("Không tìm thấy đơn hàng hoặc giả lập thất bại.");
            return Ok(new { message = $"Giả lập thanh toán THÀNH CÔNG cho đơn hàng {orderCode}!" });
        }
    }
}
