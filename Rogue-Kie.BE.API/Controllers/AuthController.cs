using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Contracts.Auth;
using Rogue_Kie.BE.Business.Services.Auth;
using Rogue_Kie.BE.Business.Services.Maintenance;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IMaintenanceService _maintenanceService;

        public AuthController(IAuthService authService, ITokenService tokenService, IMaintenanceService maintenanceService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _maintenanceService = maintenanceService;
        }

        // API gá»­i mÃ£ OTP Ä‘Äƒng kÃ½ vá» email.
        // Unity gá»i endpoint nÃ y trÆ°á»›c khi gá»i /api/auth/register.
        [HttpPost("send-register-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromBody] SendRegisterOtpRequest request)
        {
            try
            {
                // Kiá»ƒm tra validate tá»« SendRegisterOtpRequest, vÃ­ dá»¥ email báº¯t buá»™c vÃ  Ä‘Ãºng format.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiá»ƒm tra báº£o trÃ¬ há»‡ thá»‘ng
                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new SendRegisterOtpResponse
                    {
                        Success = false,
                        Message = $"Há»‡ thá»‘ng Ä‘ang báº£o trÃ¬ ({maintStatus.Title}). Vui lÃ²ng quay láº¡i sau khi báº£o trÃ¬ hoÃ n táº¥t."
                    });
                }

                await _authService.SendRegisterOtpAsync(request.Email);

                return Ok(new SendRegisterOtpResponse
                {
                    Success = true,
                    Message = "MÃ£ OTP Ä‘Ã£ Ä‘Æ°á»£c gá»­i Ä‘áº¿n email cá»§a báº¡n."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new SendRegisterOtpResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new SendRegisterOtpResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // API Ä‘Äƒng kÃ½ tÃ i khoáº£n má»›i.
        // Payload cáº§n username, email, password, confirmPassword vÃ  otpCode.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Náº¿u thiáº¿u field hoáº·c confirmPassword khÃ´ng khá»›p, dá»«ng trÆ°á»›c khi vÃ o service.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiá»ƒm tra báº£o trÃ¬ há»‡ thá»‘ng
                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new RegisterResponse
                    {
                        Success = false,
                        IsMaintenance = true,
                        Maintenance = maintStatus,
                        Message = $"Há»‡ thá»‘ng Ä‘ang báº£o trÃ¬ ({maintStatus.Title}). Vui lÃ²ng quay láº¡i sau khi báº£o trÃ¬ hoÃ n táº¥t."
                    });
                }

                var user = await _authService.RegisterAsync(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.OtpCode);

                if (user == null)
                {
                    return BadRequest(new RegisterResponse
                    {
                        Success = false,
                        Message = "KhÃ´ng thá»ƒ táº¡o tÃ i khoáº£n."
                    });
                }

                return Created($"/api/auth/register", new RegisterResponse
                {
                    Success = true,
                    Message = "ÄÄƒng kÃ½ tÃ i khoáº£n thÃ nh cÃ´ng.",
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role?.Name == null ? "User" : user.Role.Name
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new RegisterResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // API Ä‘Äƒng nháº­p.
        // Field Username cÃ³ thá»ƒ nháº­n username hoáº·c email, pháº§n service sáº½ tá»± kiá»ƒm tra cáº£ hai.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Kiá»ƒm tra request cÆ¡ báº£n trÆ°á»›c khi tÃ¬m user trong database.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _authService.LoginAsync(request.Username, request.Password);

                if (user == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "ÄÄƒng nháº­p tháº¥t báº¡i."
                    });
                }

                // Kiá»ƒm tra báº£o trÃ¬ há»‡ thá»‘ng (Chá»‰ cho phÃ©p Admin vÃ  Developer truy cáº­p khi Ä‘ang báº£o trÃ¬)
                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    string userRole = user.Role?.Name ?? "User";
                    bool isPrivileged = userRole.Equals("Admin", System.StringComparison.OrdinalIgnoreCase) ||
                                        userRole.Equals("Developer", System.StringComparison.OrdinalIgnoreCase);

                    if (!isPrivileged)
                    {
                        return StatusCode(StatusCodes.Status503ServiceUnavailable, new LoginResponse
                        {
                            Success = false,
                            IsMaintenance = true,
                            Maintenance = maintStatus,
                            Message = $"MÃ¡y chá»§ Ä‘ang báº£o trÃ¬: {maintStatus.Title}. Dá»± kiáº¿n hoÃ n táº¥t trong {maintStatus.RemainingMinutes} phÃºt ná»¯a."
                        });
                    }
                }

                // Táº¡o JWT Ä‘á»ƒ Unity lÆ°u vÃ o PlayerPrefs vÃ  gá»­i kÃ¨m Authorization cho API cáº§n Ä‘Äƒng nháº­p.
                var token = _tokenService.GenerateToken(user);
                var refreshTokenObj = await _authService.GenerateRefreshTokenAsync(user.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "ÄÄƒng nháº­p thÃ nh cÃ´ng.",
                    UserId = user.Id,
                    Username = user.Username,
                    Role = user.Role?.Name ?? "User",
                    Token = token,
                    RefreshToken = refreshTokenObj.Token
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

                [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] SendForgotPasswordOtpRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { Success = false, Message = "Email không được để trống." });
                }

                await _authService.SendForgotPasswordOtpAsync(request.Email);
                return Ok(new { Success = true, Message = "Nếu email hợp lệ, một mã OTP đã được gửi đến bạn." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return BadRequest(new { Success = false, Message = "Mật khẩu xác nhận không khớp." });
                }

                var result = await _authService.ResetPasswordAsync(request.Email, request.OtpCode, request.NewPassword);
                
                if (result)
                {
                    return Ok(new { Success = true, Message = "Đổi mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới." });
                }
                
                return BadRequest(new { Success = false, Message = "Đổi mật khẩu thất bại." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _authService.LoginWithGoogleAsync(request.IdToken);
                if (user == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Dang nhap Google that bai."
                    });
                }

                // Kiá»ƒm tra báº£o trÃ¬ há»‡ thá»‘ng (Chá»‰ cho phÃ©p Admin vÃ  Developer truy cáº­p khi Ä‘ang báº£o trÃ¬)
                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    string userRole = user.Role?.Name ?? "User";
                    bool isPrivileged = userRole.Equals("Admin", System.StringComparison.OrdinalIgnoreCase) ||
                                        userRole.Equals("Developer", System.StringComparison.OrdinalIgnoreCase);

                    if (!isPrivileged)
                    {
                        return StatusCode(StatusCodes.Status503ServiceUnavailable, new LoginResponse
                        {
                            Success = false,
                            IsMaintenance = true,
                            Maintenance = maintStatus,
                            Message = $"MÃ¡y chá»§ Ä‘ang báº£o trÃ¬: {maintStatus.Title}. Dá»± kiáº¿n hoÃ n táº¥t trong {maintStatus.RemainingMinutes} phÃºt ná»¯a."
                        });
                    }
                }

                var token = _tokenService.GenerateToken(user);
                var refreshTokenObj = await _authService.GenerateRefreshTokenAsync(user.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Dang nhap Google thanh cong.",
                    UserId = user.Id,
                    Username = user.Username,
                    Role = user.Role?.Name ?? "User",
                    Token = token,
                    RefreshToken = refreshTokenObj.Token
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh Token khÃ´ng Ä‘Æ°á»£c trá»‘ng." });
            }

            var user = await _authService.VerifyRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { Message = "Refresh Token khÃ´ng há»£p lá»‡ hoáº·c Ä‘Ã£ háº¿t háº¡n." });
            }

            // Kiá»ƒm tra báº£o trÃ¬ há»‡ thá»‘ng khi Refresh Token
            var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
            if (maintStatus.IsUnderMaintenance)
            {
                string userRole = user.Role?.Name ?? "User";
                bool isPrivileged = userRole.Equals("Admin", System.StringComparison.OrdinalIgnoreCase) ||
                                    userRole.Equals("Developer", System.StringComparison.OrdinalIgnoreCase);

                if (!isPrivileged)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                    {
                        Success = false,
                        IsMaintenance = true,
                        Maintenance = maintStatus,
                        Message = $"MÃ¡y chá»§ Ä‘ang báº£o trÃ¬ ({maintStatus.Title})."
                    });
                }
            }

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshTokenObj = await _authService.GenerateRefreshTokenAsync(user.Id);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenObj.Token,
                ExpiresAt = newRefreshTokenObj.ExpiresAt
            });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh Token khÃ´ng Ä‘Æ°á»£c trá»‘ng." });
            }

            var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            if (!success)
            {
                return BadRequest(new { Message = "KhÃ´ng tÃ¬m tháº¥y token hoáº·c token Ä‘Ã£ bá»‹ thu há»“i trÆ°á»›c Ä‘Ã³." });
            }

            return Ok(new { Message = "Thu há»“i Refresh Token thÃ nh cÃ´ng." });
        }
    }
}


