namespace Parking.Domain.Entities;

public sealed class PasswordResetToken
{
    private PasswordResetToken()
    {
    }

    public PasswordResetToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash must not be empty.", nameof(tokenHash));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("Expiration must be greater than creation time.", nameof(expiresAt));
        }

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    public bool IsUsed => UsedAt.HasValue;

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            return;
        }

        UsedAt = DateTimeOffset.UtcNow;
    }
}
