using System.ComponentModel.DataAnnotations;

namespace api.auth.models
{
    public class User
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
        [Key]
        public required string PhoneNumber { get; set; }
        public  ICollection<Wallet>? Wallets { get; set; }
        public string? Otp { get; set; }
        public DateTime OtpExpiration { get; set; }
        public bool IsOtpVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}