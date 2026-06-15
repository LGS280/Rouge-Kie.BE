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
        private readonly Rogue_Kie.BE.Business.Services.Auth.ITokenService _tokenService;

        public AuthController(IAuthService authService, Rogue_Kie.BE.Business.Services.Auth.ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _authService.RegisterAsync(request.Username, request.Password);

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
                    Username = user.Username
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
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

                var token = _tokenService.GenerateToken(user);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    UserId = user.Id,
                    Username = user.Username,
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
