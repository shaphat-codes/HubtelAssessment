using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using api.Data;
using api.wallet.repository;
using api.wallet.services;
using api.auth.models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace WalletServiceTests
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly WalletService _walletService;
        private readonly AppDbContext _dbContext;

        public WalletServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dbContext = new AppDbContext(options);

            _walletRepositoryMock = new Mock<IWalletRepository>();
            var loggerMock = new Mock<ILogger<WalletService>>();
            _walletService = new WalletService(_walletRepositoryMock.Object, _dbContext, loggerMock.Object);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WalletExists_ReturnsWallet()
        {
            var walletId = Guid.NewGuid();
            var wallet = new Wallet { ID = walletId, AccountNumber = "1234567890123456", Owner = "1234567890" };
            _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

            var result = await _walletService.GetWalletByIdAsync(walletId);

            Assert.NotNull(result);
            Assert.Equal(walletId, result.ID);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WalletDoesNotExist_ThrowsKeyNotFoundException()
        {
            var walletId = Guid.NewGuid();
            _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _walletService.GetWalletByIdAsync(walletId));
        }

        [Fact]
        public async Task AddWalletAsync_ValidWallet_AddsWallet()
        {
            var wallet = new Wallet { ID = Guid.NewGuid(), Owner = "1234567890", Type = WalletType.Momo, AccountNumber = "1234567890123456" };
            var user = new User { PhoneNumber = "1234567890", FirstName = "John", LastName = "Doe" };
            _walletRepositoryMock.Setup(repo => repo.WalletExistsAsync(wallet.AccountNumber)).ReturnsAsync(false);
            _walletRepositoryMock.Setup(repo => repo.GetUserWalletCountAsync(wallet.Owner)).ReturnsAsync(0);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            await _walletService.AddWalletAsync(wallet);

            _walletRepositoryMock.Verify(repo => repo.AddWalletAsync(wallet), Times.Once);
        }

        [Fact]
        public async Task AddWalletAsync_DuplicateWallet_ThrowsInvalidOperationException()
        {
            var wallet = new Wallet { ID = Guid.NewGuid(), Owner = "1234567890", Type = WalletType.Momo, AccountNumber = "1234567890123456" };
            _walletRepositoryMock.Setup(repo => repo.WalletExistsAsync(wallet.AccountNumber)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.AddWalletAsync(wallet));
        }

        [Fact]
        public async Task DeleteWalletAsync_WalletExists_DeletesWallet()
        {
            var walletId = Guid.NewGuid();
            var wallet = new Wallet { ID = walletId, AccountNumber = "1234567890123456", Owner = "1234567890" };
            _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

            await _walletService.DeleteWalletAsync(walletId);

            _walletRepositoryMock.Verify(repo => repo.DeleteWalletAsync(walletId), Times.Once);
        }

        [Fact]
        public async Task DeleteWalletAsync_WalletDoesNotExist_ThrowsKeyNotFoundException()
        {
            var walletId = Guid.NewGuid();
            _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _walletService.DeleteWalletAsync(walletId));
        }
    }
}