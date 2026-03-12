namespace Voltiq.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsActive { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    private RefreshToken() { }

    public static RefreshToken Create(string token, Guid userId, int expiresInDays) =>
        new()
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsActive = true,
        };

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        IsRevoked = true;
        IsActive = false;
    }
}
