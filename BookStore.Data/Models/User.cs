using System;
using System.Collections.Generic;

namespace BookStore.Data.Models;

public partial class User
{
    public int UserId { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Gender { get; set; }

    public string UserName { get; set; } = null!;

    public byte[] PasswordHash { get; set; }=new byte[32];

    public byte[] PasswordSalt { get; set; } = new byte[32];

    public string? VerificationToken { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }

    public string Email { get; set; } = null!;

    public string? Role { get; set; }

    public virtual ICollection<OrderBook> OrderBooks { get; } = new List<OrderBook>();
}
