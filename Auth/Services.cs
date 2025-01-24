using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using api.auth.models;
using api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.auth.services
{
    public interface IAuthService
    {
        Task<string> SignUpAsync(User user);
        Task<string> SignInAsync(string phoneNumber);
        Task<string> VerifyOtpAndSignInAsync(string phoneNumber, string otp);
        
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

         public async Task<string> SignUpAsync(User user)
        {
            _logger.LogInformation("Attempting to sign up user with phone number: {PhoneNumber}", user.PhoneNumber);

            if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
            {
                _logger.LogWarning("User already exists with phone number: {PhoneNumber}", user.PhoneNumber);
                throw new InvalidOperationException("User already exists.");
            }

            var otp = await SendOtpAsync(user.PhoneNumber);

            user.Otp = otp;
            user.OtpExpiration = DateTime.UtcNow.AddHours(5);
            user.IsOtpVerified = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP sent to user with phone number: {PhoneNumber}", user.PhoneNumber);
            return $"OTP sent. Please verify to complete registration. OTP: {otp}";
        }

        public async Task<string> SignInAsync(string phoneNumber)
        {
            _logger.LogInformation("Attempting to sign in user with phone number: {PhoneNumber}", phoneNumber);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                _logger.LogWarning("User not found with phone number: {PhoneNumber}", phoneNumber);
                throw new InvalidOperationException("User not found.");
            }

            if (!user.IsOtpVerified)
            {
                _logger.LogWarning("OTP not verified for user with phone number: {PhoneNumber}", phoneNumber);
                throw new InvalidOperationException("OTP not verified.");
            }

            _logger.LogInformation("User signed in successfully with phone number: {PhoneNumber}", phoneNumber);
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("JWT key is not configured.");
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var keyBytes = Encoding.ASCII.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.PhoneNumber)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            _logger.LogInformation("JWT token generated successfully for user with phone number: {PhoneNumber}", user.PhoneNumber);
            return tokenHandler.WriteToken(token);
        }


        public async Task<string> SendOtpAsync(string phoneNumber)
        {
            var otp = GenerateOtp();
            _logger.LogInformation("Generated OTP: {Otp} for phone number: {PhoneNumber}", otp, phoneNumber);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                user.Otp = otp;
                user.OtpExpiration = DateTime.UtcNow.AddHours(5);
                await _context.SaveChangesAsync();
            }

            var clientSecret = _configuration["Hubtel:ClientSecret"];
            var clientId = _configuration["Hubtel:ClientId"];
            var from = _configuration["Hubtel:From"];
            var url = $"https://smsc.hubtel.com/v1/messages/send?clientsecret={clientSecret}&clientid={clientId}&from={from}&to={phoneNumber}&content=Your+OTP+is+{otp}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send OTP to phone number: {PhoneNumber}", phoneNumber);
                throw new InvalidOperationException("Failed to send OTP.");
            }

            _logger.LogInformation("OTP sent successfully to phone number: {PhoneNumber}", phoneNumber);
            return otp;
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null || user.Otp != otp || user.OtpExpiration < DateTime.UtcNow)
            {
                return false;
            }

            user.Otp = null;
            user.OtpExpiration = DateTime.MinValue;
            user.IsOtpVerified = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> VerifyOtpAndSignInAsync(string phoneNumber, string otp)
        {
            if (!await VerifyOtpAsync(phoneNumber, otp))
            {
                throw new InvalidOperationException("Invalid OTP.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            return GenerateJwtToken(user);
        }

        private string GenerateOtp()
        {
            // Implement OTP generation logic here
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}