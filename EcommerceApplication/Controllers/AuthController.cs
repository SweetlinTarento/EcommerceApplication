using EcommerceApplication.DTO;
using EcommerceApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService,ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                _logger.LogWarning("Registration failed: Email and password are required");
                return BadRequest(new { error = "Email and password are required" });
            }
            try
            {
                var result = await _authService.RegisterAsync(dto);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Registration failed for email: {Email}. Reason: {Reason}", dto.Email, result.Message);
                    return BadRequest(new { error = result.Message });
                }
                
                return Ok(new { message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user with email: {Email}", dto.Email);
                return StatusCode(500, new {error="Internal server error"});
            }
        }

        [Authorize]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
           
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                _logger.LogWarning("Login failed: Email and password are required");
                return BadRequest(new { error = "Email and password are required" });
            }
            try
            {
                var result = await _authService.LoginAsync(dto);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", dto.Email, result.Message);
                    return Unauthorized(new { error = result.Message });
                }
                
                return Ok(new
                {
                    token = result.Token,
                    refreshToken = result.RefreshToken,
                    expiresIn = result.ExpiresIn,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user with email: {Email}", dto.Email);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token failed: Token and refresh token are required");
                return BadRequest(new { error = "Token and refresh token are required" });
            }
            try
            {
                var result = await _authService.RefreshTokenAsync(request);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Refresh token failed for token: {Token}. Reason: {Reason}", request.Token, result.Message);
                    return Unauthorized(new { error = result.Message });
                }
                
                return Ok(new
                {
                    token = result.Token,
                    refreshToken = result.RefreshToken,
                    expiresIn = result.ExpiresIn,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing token for token: {Token}", request.Token);
                return StatusCode(500, new { error = "Internal Server Error" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {   _logger.LogWarning("Logout failed: User ID not found in claims");
                    return BadRequest(new { error = "User not found" });
                }

                var result = await _authService.RevokeTokenAsync(userId);
                if (!result)
                {
                    _logger.LogWarning("Logout failed for user ID: {UserId}", userId);
                    return BadRequest(new { error = "Logout failed" });
                }

                
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out user");
                return StatusCode(500,new { error = ex.Message });
            }
        }
    }
}