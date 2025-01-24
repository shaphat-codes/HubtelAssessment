using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.auth.models;
using api.auth.services;

namespace api.auth.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            try
            {
                var token = await _authService.SignUpAsync(user);
                var response = new ApiResponse<string>(true, "User signed up successfully", token);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return BadRequest(response);
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var errorResponse = new ApiResponse<string>(false, "Phone number is required", null);
                    return BadRequest(errorResponse);
                }
                var token = await _authService.SignInAsync(model.PhoneNumber);
                var response = new ApiResponse<string>(true, "User signed in successfully", token);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return BadRequest(response);
            }
        }

        [HttpPost("verify-otp-signin")]
        public async Task<IActionResult> VerifyOtpAndSignIn([FromBody] OtpVerificationModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var errorResponse = new ApiResponse<string>(false, "Phone number is required", null);
                    return BadRequest(errorResponse);
                }
                if (string.IsNullOrEmpty(model.Otp))
                {
                    var errorResponse = new ApiResponse<string>(false, "OTP is required", null);
                    return BadRequest(errorResponse);
                }
                var token = await _authService.VerifyOtpAndSignInAsync(model.PhoneNumber, model.Otp);
                var response = new ApiResponse<string>(true, "OTP verified and user signed in successfully", token);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return BadRequest(response);
            }
        }
    }

    public class SignInModel
    {
        public string? PhoneNumber { get; set; }
    }

    public class OtpVerificationModel
    {
        public string? PhoneNumber { get; set; }
        public string? Otp { get; set; }
    }
}