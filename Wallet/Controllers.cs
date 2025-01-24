using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api.wallet.services;
using System.Security.Claims;

namespace api.wallet.controllers
{
    [ApiController]
    [Route("api/v1/wallets")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> AddWallet([FromBody] Wallet wallet)
        {
            try
            {
                var phoneNumber = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    var errorResponse = new ApiResponse<string>(false, "User phone number not found in token", null);
                    return BadRequest(errorResponse);
                }

                wallet.Owner = phoneNumber;

                await _walletService.AddWalletAsync(wallet);
                var response = new ApiResponse<Wallet>(true, "Wallet created successfully", wallet);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return BadRequest(response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(Guid id)
        {
            try
            {
                await _walletService.DeleteWalletAsync(id);
                var response = new ApiResponse<string>(true, "Wallet deleted successfully", null);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return NotFound(response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(Guid id)
        {
            try
            {
                var wallet = await _walletService.GetWalletByIdAsync(id);
                var response = new ApiResponse<Wallet>(true, "Wallet retrieved successfully", wallet);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                var response = new ApiResponse<string>(false, ex.Message, null);
                return NotFound(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var phoneNumber = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(phoneNumber))
            {
                var errorResponse = new ApiResponse<string>(false, "User phone number not found in token", null);
                return BadRequest(errorResponse);
            }

            var wallets = await _walletService.GetAllWalletsAsync(phoneNumber);
            var response = new ApiResponse<IEnumerable<Wallet>>(true, "Wallets retrieved successfully", wallets);
            return Ok(response);
        }
    }
}