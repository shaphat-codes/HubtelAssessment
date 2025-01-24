using api.Data;
using api.wallet.repository;

namespace api.wallet.services
{
    public interface IWalletService
    {
        Task<Wallet> GetWalletByIdAsync(Guid id);
        Task<IEnumerable<Wallet>> GetAllWalletsAsync();
        Task AddWalletAsync(Wallet wallet);
        Task DeleteWalletAsync(Guid id);
    }


    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(IWalletRepository walletRepository, AppDbContext context, ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<Wallet> GetWalletByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting wallet by ID: {Id}", id);
            var wallet = await _walletRepository.GetWalletByIdAsync(id);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found: {Id}", id);
                throw new KeyNotFoundException("Wallet not found.");
            }
            return wallet;
        }

        public async Task<IEnumerable<Wallet>> GetAllWalletsAsync()
        {
            _logger.LogInformation("Getting all wallets");
            return await _walletRepository.GetAllWalletsAsync();
        }

        public async Task AddWalletAsync(Wallet wallet)
        {
            _logger.LogInformation("Adding wallet for owner: {Owner}", wallet.Owner);
            if (await _walletRepository.WalletExistsAsync(wallet.AccountNumber))
            {
                _logger.LogWarning("Duplicate wallet addition attempted for account number: {AccountNumber}", wallet.AccountNumber);
                throw new InvalidOperationException("Duplicate wallet addition is not allowed.");
            }

            if (await _walletRepository.GetUserWalletCountAsync(wallet.Owner) >= 5)
            {
                _logger.LogWarning("User {Owner} has reached the maximum number of wallets", wallet.Owner);
                throw new InvalidOperationException("A user cannot have more than 5 wallets.");
            }

            if (wallet.Type == WalletType.Card)
            {
                wallet.AccountNumber = wallet.AccountNumber.Substring(0, 6);
            }

            var user = await _context.Users.FindAsync(wallet.Owner);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Owner}", wallet.Owner);
                throw new InvalidOperationException("User not found.");
            }

            wallet.User = user;

            await _walletRepository.AddWalletAsync(wallet);
            _logger.LogInformation("Wallet added successfully for owner: {Owner}", wallet.Owner);
        }

        public async Task DeleteWalletAsync(Guid id)
        {
            _logger.LogInformation("Deleting wallet by ID: {Id}", id);
            var wallet = await _walletRepository.GetWalletByIdAsync(id);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found: {Id}", id);
                throw new KeyNotFoundException("Wallet not found.");
            }

            await _walletRepository.DeleteWalletAsync(id);
            _logger.LogInformation("Wallet deleted successfully: {Id}", id);
        }
    }
}