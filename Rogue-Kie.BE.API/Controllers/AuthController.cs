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

        // Send OTP code to email for registration.
        [HttpPost("send-register-otp")]
        public async Task<IActionResult> SendRegisterOtp([FromBody] SendRegisterOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new SendRegisterOtpResponse
                    {
                        Success = false,
                        Message = $"Server is under maintenance ({maintStatus.Title}). Please try again after maintenance completes."
                    });
                }

                await _authService.SendRegisterOtpAsync(request.Email);

                return Ok(new SendRegisterOtpResponse
                {
                    Success = true,
                    Message = "OTP code has been sent to your email."
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

        // Register a new account.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var maintStatus = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
                if (maintStatus.IsUnderMaintenance)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new RegisterResponse
                    {
                        Success = false,
                        IsMaintenance = true,
                        Maintenance = maintStatus,
                        Message = $"Server is under maintenance ({maintStatus.Title}). Please try again after maintenance completes."
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
                        Message = "Failed to create account."
                    });
                }

                return Created($"/api/auth/register", new RegisterResponse
                {
                    Success = true,
                    Message = "Account registered successfully.",
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

        // Login user.
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
                        Message = "Login failed."
                    });
                }

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
                            Message = $"Server is under maintenance: {maintStatus.Title}. Expected completion in {maintStatus.RemainingMinutes} minutes."
                        });
                    }
                }

                var token = _tokenService.GenerateToken(user);
                var refreshTokenObj = await _authService.GenerateRefreshTokenAsync(user.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful.",
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
                    return BadRequest(new { Success = false, Message = "Email cannot be empty." });
                }

                await _authService.SendForgotPasswordOtpAsync(request.Email);
                return Ok(new { Success = true, Message = "If the email is valid, an OTP code has been sent to you." });
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
                    return BadRequest(new { Success = false, Message = "Confirm password does not match." });
                }

                var result = await _authService.ResetPasswordAsync(request.Email, request.OtpCode, request.NewPassword);
                
                if (result)
                {
                    return Ok(new { Success = true, Message = "Password reset successful. You can now log in with your new password." });
                }
                
                return BadRequest(new { Success = false, Message = "Failed to reset password." });
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
                        Message = "Google login failed."
                    });
                }

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
                            Message = $"Server is under maintenance: {maintStatus.Title}. Expected completion in {maintStatus.RemainingMinutes} minutes."
                        });
                    }
                }

                var token = _tokenService.GenerateToken(user);
                var refreshTokenObj = await _authService.GenerateRefreshTokenAsync(user.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Google login successful.",
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
                return BadRequest(new { Message = "Refresh Token cannot be empty." });
            }

            var user = await _authService.VerifyRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { Message = "Refresh Token is invalid or has expired." });
            }

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
                        Message = $"Server is under maintenance ({maintStatus.Title})."
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
                return BadRequest(new { Message = "Refresh Token cannot be empty." });
            }

            var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            if (!success)
            {
                return BadRequest(new { Message = "Token not found or has already been revoked." });
            }

            return Ok(new { Message = "Refresh Token revoked successfully." });
        }
    }
}
