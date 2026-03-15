using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using Voltiq.Domain.Interfaces;

namespace Voltiq.Infrastructure.Auth;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 32;
    private const int ITERATIONS = 4;
    private const int MEMORY_SIZE = 65536; // 64 MB
    private const int DEGREE_OF_PARALLELISM = 2;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        var hash = ComputeHash(password, salt);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash = ComputeHash(password, salt);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = ITERATIONS,
            MemorySize = MEMORY_SIZE,
            DegreeOfParallelism = DEGREE_OF_PARALLELISM
        };

        return argon2.GetBytes(HASH_SIZE);
    }
}
