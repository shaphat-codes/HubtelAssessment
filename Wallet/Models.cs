using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.auth.models;

public enum WalletType
{
    Momo,
    Card
}

public enum AccountScheme
{
    Visa,
    Mastercard,
    Mtn,
    Vodafone,
    Airteltigo
}

public class Wallet
{
    public Guid ID { get; set; }
    public WalletType Type { get; set; }
    public required string AccountNumber { get; set; }
    public AccountScheme Scheme { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public required string Owner { get; set; }

    [ForeignKey("Owner")]
    public User? User { get; set; }

}