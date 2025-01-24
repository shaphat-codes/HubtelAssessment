using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api.wallet.repository
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByIdAsync(Guid id);
        Task<IEnumerable<Wallet>> GetAllWalletsAsync();
        Task AddWalletAsync(Wallet wallet);
        Task DeleteWalletAsync(Guid id);
        Task<bool> WalletExistsAsync(string accountNumber);
        Task<int> GetUserWalletCountAsync(string owner);
    }


     public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetWalletByIdAsync(Guid id)
        {
            var wallet = await _context.Wallets.FindAsync(id);
            if (wallet == null)
            {
                return null;
            }
            return wallet;
        }

        public async Task<IEnumerable<Wallet>> GetAllWalletsAsync()
        {
            return await _context.Wallets.ToListAsync();
        }

        public async Task AddWalletAsync(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWalletAsync(Guid id)
        {
            var wallet = await _context.Wallets.FindAsync(id);
            if (wallet != null)
            {
                _context.Wallets.Remove(wallet);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> WalletExistsAsync(string accountNumber)
        {
            return await _context.Wallets.AnyAsync(w => w.AccountNumber == accountNumber);
        }

        public async Task<int> GetUserWalletCountAsync(string owner)
        {
            return await _context.Wallets.CountAsync(w => w.Owner == owner);
        }
    }
}