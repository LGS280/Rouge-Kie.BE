using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Contracts.Auth;
using Rogue_Kie.BE.Business.Services.Auth;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        // API gửi mã OTP đăng ký về email.
        // Unity gọi endpoint này trước khi gọi /api/auth/register.
        [HttpPost("send-register-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromBody] SendRegisterOtpRequest request)
        {
            try
            {
                // Kiểm tra validate từ SendRegisterOtpRequest, ví dụ email bắt buộc và đúng format.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.SendRegisterOtpAsync(request.Email);

                return Ok(new SendRegisterOtpResponse
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn."
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

        // API đăng ký tài khoản mới.
        // Payload cần username, email, password, confirmPassword và otpCode.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Nếu thiếu field hoặc confirmPassword không khớp, dừng trước khi vào service.
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
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
                        Message = "Không thể tạo tài khoản."
                    });
                }

                return Created($"/api/auth/register", new RegisterResponse
                {
                    Success = true,
                    Message = "Đăng ký tài khoản thành công.",
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

        // API đăng nhập.
        // Field Username có thể nhận username hoặc email, phần service sẽ tự kiểm tra cả hai.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Kiểm tra request cơ bản trước khi tìm user trong database.
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
                        Message = "Đăng nhập thất bại."
                    });
                }

                // Tạo JWT để Unity lưu vào PlayerPrefs và gửi kèm Authorization cho API cần đăng nhập.
                var token = _tokenService.GenerateToken(user);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    UserId = user.Id,
                    Username = user.Username,
                    Role = user.Role?.Name ?? "User",
                    Token = token
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
    }
}

