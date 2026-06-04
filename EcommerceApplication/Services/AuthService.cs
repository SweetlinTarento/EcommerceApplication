using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;


namespace EcommerceApplication.Services
{
    public class AuthService : IAuthService
    {
        bool value = false, value1 = false;
        private readonly UserManager<ApplicationUser> _userManager;//UserManager is a package for managing users in application used for creating users, deleting updating, checking passwords, etc.
        public readonly IConfiguration _configuration;//to read application settings like JWT secret key, issuer, audience, etc.
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthService> logger  )
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }
        private string IsValidPasswordFormat(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                value = false;
                return "Password length is less than 8 characters";
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                value = false;
                return "Password must conatain atleast one capital letter";
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                value = false;
                return "Password must contain atleast one small letter";
            }
               

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                value = false;
                return "Password must contain atleast one number";
            }


            if (!Regex.IsMatch(password, @"[*$%&!@(){}#]"))
            {
                value = false;
                return "Password must contain atleast one non-alphanumerical";
            }
            value = true;
            return "Password is correct";
        }
        public string IsValidateEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                value1 = false;
                return "Email is required.";
            }
                
            email = email.Trim();

            if (email.Length < 8)
            {
                value1 = false;
                return "Email must be at least 8 characters long.";

            }

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, pattern))
            {
                value1 = false;
                return "Invalid email format.";
            }
            value1 = true;
            return "Email is correct"; // valid email
        }
        private string GenerateAccessToken(ApplicationUser user, List<string> roles)
        {
            try
            {
                _logger.LogInformation("Generating JWT token for user: {UserId}", user.Id);
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("JWT token generated successfully for user: {UserId}", user.Id);

                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating JWT token for user: {UserId}", user.Id);
                throw;
            }
        }

        // Generates a refresh token (cryptographically secure random string)
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterDTO dto)
            {
            string message = IsValidateEmailFormat(dto.Email);
            if (!value1)
            {
                _logger.LogWarning("Email format is invalid");
                return new AuthResult
                {
                    Succeeded = false,
                    Message = message
                };
            }
            string message1 = IsValidPasswordFormat(dto.Password);
            if (!value)
            {
                _logger.LogWarning("Password format is invalid for email: {Email}", dto.Email);
                return new AuthResult
                {
                    Succeeded = false,
                    Message = message1
                };
            }
            var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email
                    //RefreshToken=GenerateRefreshToken()
            };
                
                var result = await _userManager.CreateAsync(user, dto.Password);//CreateAsync is a method to create a new user with the specified password, it will hash the password and store it securely in the database
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for email: {Email}. Errors: {Errors}",dto.Email, errorMessages);
                return new AuthResult
                {
                    Succeeded = false,
                    Message = errorMessages
                };
            }
            _logger.LogInformation("User registered successfully with email: {Email}", dto.Email);
            //await _userManager.AddToRoleAsync(user, "CUSTOMER");
            await _userManager.AddToRoleAsync(user, "ADMIN");
            return new AuthResult
            {
                Succeeded = true,
                Message = "User registered successfully",
                User = user
            };
        }
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed for email: {Email}. User not found.", dto.Email);
                    throw new Exception("Invalid email");
                }
                    

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login attempt failed for email: {Email}. Invalid password.", dto.Email);
                    throw new Exception("Invalid password");
                }

                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User logged in successfully with email: {Email} and generating token.", dto.Email);

                // Generate access token
                var accessToken = GenerateAccessToken(user, roles.ToList());

                // Generate refresh token
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                // Save refresh token and expiry to database
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Refresh token generated and saved successfully for user: {Email}", dto.Email);

                return new AuthResponseDTO
                {
                    Succeeded = true,
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = DateTime.UtcNow.AddHours(1),
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email: {Email}", dto.Email);
                return new AuthResponseDTO
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {//used to refresh tokens when they expire especially access token having 1 hour validity, so instead of the user logging in again, this function is called
            try
            {
                var principal = GetPrincipalFromExpiredToken(request.Token);//principle is a class to represent the user and their claims,
                                                                            //GetPrincipalFromExpiredToken is a method to extract claims from an expired token without validating lifetime
                if (principal == null)//checks if the JWT is fake, malformed or tampered
                {
                    _logger.LogWarning("Invalid token provided for refresh.");
                    return new AuthResponseDTO
                    {
                        Succeeded = false,
                        Message = "Invalid token"
                    };
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;//get the userId from the claims
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.RefreshToken != request.RefreshToken)//checking if the existing refresh token matches the one in the database for that user, if not then it is invalid
                {
                    _logger.LogWarning("Invalid refresh token provided for user ID: {UserId}", userId);
                    return new AuthResponseDTO
                    {
                        Succeeded = false,
                        Message = "Invalid refresh token"
                    };
                }

                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)//if refresh token expired, have to login again
                {
                    _logger.LogWarning("Refresh token expired for user ID: {UserId}", userId);
                    return new AuthResponseDTO
                    {
                        Succeeded = false,
                        Message = "Refresh token has expired"
                    };
                }
                //access token is how long the user can access the page while refresh token is to keep the logging for a few days, if not opened the page within 7 days, then new refresh token is to be generated by logging in again.
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = GenerateAccessToken(user, roles.ToList());
                var newRefreshToken = GenerateRefreshToken();//refresh token is generated again to prevent reuse of the same refresh token, which is a security risk
                var newRefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = newRefreshTokenExpiryTime;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Token refreshed successfully for user ID: {UserId}", userId);
                return new AuthResponseDTO
                {
                    Succeeded = true,
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = DateTime.UtcNow.AddHours(1),
                    Message = "Token refreshed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing token.");
                return new AuthResponseDTO
                {
                    Succeeded = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<bool> RevokeTokenAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempt to revoke token failed. User not found with ID: {UserId}", userId);
                    return false;
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Token revoked successfully for user ID: {UserId}", userId);
                return true;
            }
            catch
            {
                _logger.LogError("An error occurred while revoking token for user ID: {UserId}", userId);
                return false;
            }
        }

        /// Extracts claims from an expired token without validating lifetime
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var tokenHandler = new JwtSecurityTokenHandler();//JwtSecurityTokenHandler is a class to create read and validate JWT tokens

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,//if signature is valid
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false // Allow expired tokens
                }, out SecurityToken securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || //checks if the algorithm used is HMAC SHA256
                   !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("JWT rejected due to invalid signing algorithm or token structure");
                    return null;
                }

                return principal;
            }
            catch(Exception ex) 
            {
                _logger.LogWarning(ex, "JWT validation failed (token may be expired, malformed, or tampered)");
                return null;
            }
        }
    }
}