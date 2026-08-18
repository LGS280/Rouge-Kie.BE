using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.Payment;
using Rogue_Kie.BE.Contracts.Payment;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller xử lý Cổng Thanh Toán PayOS VietQR & Nạp Vật Phẩm Game (Real-time Payment Integration).
    /// Hỗ trợ sinh VietQR Code, tiếp nhận Webhook ngân hàng tự động, Polling kiểm tra trạng thái và Hủy đơn rác hàng loạt.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;

        /// <summary>
        /// Dependency Injection cho PayOS Payment Service
        /// </summary>
        public PaymentController(IPayOSService payOSService)
        {
            _payOSService = payOSService;
        }

        /// <summary>
        /// Endpoint: POST /api/payment/create-payment-link
        /// Tạo link thanh toán PayOS & Sinh mã VietQR Code cho người chơi nạp Gem/Vàng/Gói vật phẩm.
        /// Tự động liên kết tài khoản UserId, tạo đơn hàng trạng thái PENDING trong Database và mã hóa chữ ký HMAC-SHA256 gửi PayOS.
        /// </summary>
        [HttpPost("create-payment-link")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
        {
            // Trích xuất UserId từ JWT Token Claims
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin người dùng hợp lệ.");
            }

            // Gọi Service khởi tạo đơn nạp VietQR PayOS
            var response = await _payOSService.CreatePaymentLinkAsync(userId, request);
            return Ok(response);
        }

        /// <summary>
        /// Endpoint: POST /api/payment/payos-webhook
        /// Webhook Callback tự động từ PayOS Server gửi về khi ngân hàng xác nhận giao dịch chuyển khoản thành công.
        /// Tự động cập nhật đơn hàng thành SUCCESS và nạp số Gem/Vàng tương ứng vào tài khoản người chơi ngầm.
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
        /// Endpoint: GET /api/payment/check-status/{orderCode}
        /// Kiểm tra trạng thái đơn hàng nạp tiền (Dành cho cơ chế Polling 2.5s từ Unity Client / Web Admin UI).
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
        /// Endpoint: POST /api/payment/cancel/{orderCode}
        /// Hủy một đơn hàng nạp tiền cụ thể đang ở trạng thái PENDING trên PayOS Server và Database.
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
        /// Endpoint: POST /api/payment/cancel-all-pending
        /// Hủy TOÀN BỘ các đơn hàng nạp tiền đang bị treo PENDING của người chơi hiện tại trên PayOS Server và DB, tránh nghẽn mã đơn.
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
        /// Endpoint: POST /api/payment/dev-simulate-success/{orderCode}
        /// Giả lập thanh toán thành công trong môi trường kiểm thử (Môi trường Dev / Testing không cần tốn tiền thật).
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
