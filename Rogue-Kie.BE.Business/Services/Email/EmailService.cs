using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Rogue_Kie.BE.Business.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Gửi email chứa mã OTP đăng ký.
        // Nếu lỗi ở đây, kiểm tra EmailSettings trong appsettings.json và quyền SMTP của tài khoản mail.
        public async Task SendRegisterOtpAsync(string toEmail, string otpCode)
        {
            var fromEmail = _config["EmailSettings:FromEmail"] ?? _config["EmailSettings:Username"];
            var fromName = _config["EmailSettings:FromName"] ?? "Rogue-Kie";
            var host = _config["EmailSettings:Host"];
            var port = int.TryParse(_config["EmailSettings:Port"], out var p) ? p : 587;
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];

            // Thiếu username/password SMTP thì không thể gửi OTP.
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Cấu hình SMTP chưa được thiết lập trong appsettings.json.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Mã OTP đăng ký tài khoản Rogue-Kie";

            // Thiết kế giao diện HTML cho Email
            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                    <h2 style='color: #333; text-align: center;'>Xác Thực Đăng Ký Tài Khoản</h2>
                    <p>Chào bạn,</p>
                    <p>Mã OTP để hoàn tất quá trình đăng ký tài khoản Rogue-Kie của bạn là:</p>
                    <div style='background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 26px; font-weight: bold; letter-spacing: 4px; color: #ff4757; margin: 20px 0; border-radius: 4px;'>
                        {otpCode}
                    </div>
                    <p style='color: #666; font-size: 14px;'>Mã này có hiệu lực trong vòng <b>5 phút</b>. Vui lòng tuyệt đối không chia sẻ mã này cho bất kỳ ai khác.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='color: #999; font-size: 12px; text-align: center;'>Đây là email tự động từ hệ thống, vui lòng không phản hồi lại email này.</p>
                </div>";

            message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            try
            {
                // Kết nối SMTP, đăng nhập, gửi mail rồi đóng kết nối.
                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // In log lỗi chi tiết ra console để dễ debug (giống MovieTheater)
                Console.WriteLine($"[EmailService Error]: {ex.Message}");

                // Ném tiếp ngoại lệ ra ngoài để AuthService nhận biết được việc gửi Mail thất bại và chặn việc lưu DB
                throw new InvalidOperationException("Hệ thống không thể gửi email OTP lúc này. Vui lòng thử lại sau.");
            }
        }
    }
}
