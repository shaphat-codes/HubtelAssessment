// using Xunit;
// using Moq;
// using api.Data;
// using api.wallet.repository;

// namespace WalletServiceTests
// {
//     public class WalletServiceTests
//     {
//         private readonly Mock<IWalletRepository> _walletRepositoryMock;
//         private readonly Mock<AppDbContext> _contextMock;
//         private readonly Mock<ILogger<WalletService>> _loggerMock;
//         private readonly WalletService _walletService;

//         public WalletServiceTests()
//         {
//             _walletRepositoryMock = new Mock<IWalletRepository>();
//             _contextMock = new Mock<AppDbContext>();
//             _loggerMock = new Mock<ILogger<WalletService>>();
//             _walletService = new WalletService(_walletRepositoryMock.Object, _contextMock.Object, _loggerMock.Object);
//         }

//         [Fact]
//         public async Task GetWalletByIdAsync_WalletExists_ReturnsWallet()
//         {
//             // Arrange
//             var walletId = Guid.NewGuid();
//             var wallet = new Wallet { ID = walletId, AccountNumber = "1234567890123456", Owner = "1234567890" };
//             _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

//             // Act
//             var result = await _walletService.GetWalletByIdAsync(walletId);

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(walletId, result.ID);
//         }

//         [Fact]
//         public async Task GetWalletByIdAsync_WalletDoesNotExist_ThrowsKeyNotFoundException()
//         {
//             // Arrange
//             var walletId = Guid.NewGuid();
//             _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet)null);

//             // Act & Assert
//             await Assert.ThrowsAsync<KeyNotFoundException>(() => _walletService.GetWalletByIdAsync(walletId));
//         }

//         [Fact]
//         public async Task AddWalletAsync_ValidWallet_AddsWallet()
//         {
//             // Arrange
//             var wallet = new Wallet { ID = Guid.NewGuid(), Owner = "1234567890", Type = WalletType.Momo, AccountNumber = "1234567890123456" };
//             var user = new User { PhoneNumber = "1234567890" };
//             _walletRepositoryMock.Setup(repo => repo.WalletExistsAsync(wallet.AccountNumber)).ReturnsAsync(false);
//             _walletRepositoryMock.Setup(repo => repo.GetUserWalletCountAsync(wallet.Owner)).ReturnsAsync(0);
//             _contextMock.Setup(context => context.Users.FindAsync(wallet.Owner)).ReturnsAsync(user);

//             // Act
//             await _walletService.AddWalletAsync(wallet);

//             // Assert
//             _walletRepositoryMock.Verify(repo => repo.AddWalletAsync(wallet), Times.Once);
//         }

//         [Fact]
//         public async Task AddWalletAsync_DuplicateWallet_ThrowsInvalidOperationException()
//         {
//             // Arrange
//             var wallet = new Wallet { ID = Guid.NewGuid(), Owner = "1234567890", Type = WalletType.Momo, AccountNumber = "1234567890123456" };
//             _walletRepositoryMock.Setup(repo => repo.WalletExistsAsync(wallet.AccountNumber)).ReturnsAsync(true);

//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.AddWalletAsync(wallet));
//         }

//         [Fact]
//         public async Task DeleteWalletAsync_WalletExists_DeletesWallet()
//         {
//             // Arrange
//             var walletId = Guid.NewGuid();
//             var wallet = new Wallet { ID = walletId };
//             _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

//             // Act
//             await _walletService.DeleteWalletAsync(walletId);

//             // Assert
//             _walletRepositoryMock.Verify(repo => repo.DeleteWalletAsync(walletId), Times.Once);
//         }

//         [Fact]
//         public async Task DeleteWalletAsync_WalletDoesNotExist_ThrowsKeyNotFoundException()
//         {
//             // Arrange
//             var walletId = Guid.NewGuid();
//             _walletRepositoryMock.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet)null);

//             // Act & Assert
//             await Assert.ThrowsAsync<KeyNotFoundException>(() => _walletService.DeleteWalletAsync(walletId));
//         }
//     }
// }