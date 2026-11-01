using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Echo.Auth.Invitations;

/// <summary>
/// Encrypts invitation codes (AES-256-GCM) so the list can show and copy them again. The key comes
/// from configuration, so codes stay readable across restarts and deployments. Only the encrypted
/// form is stored; the hash used to look a code up is separate.
/// </summary>
public class InvitationCodeProtector
{
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public InvitationCodeProtector(IOptions<InvitationOptions> options)
    {
        if (!TryReadKey(options.Value.CodeKey, out _key))
            throw new InvalidOperationException(
                $"Invitations:CodeKey must be the Base64 of exactly {KeySize} bytes."
            );
    }

    /// <summary>Encrypts a code. The result is Base64 of nonce, tag and cipher text.</summary>
    public string Protect(string code)
    {
        var plain = System.Text.Encoding.UTF8.GetBytes(code);
        var blob = new byte[NonceSize + TagSize + plain.Length];
        var nonce = blob.AsSpan(0, NonceSize);
        var tag = blob.AsSpan(NonceSize, TagSize);
        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plain, blob.AsSpan(NonceSize + TagSize), tag);
        return Convert.ToBase64String(blob);
    }

    /// <summary>
    /// The code, or null when there is none or it cannot be read (for example the key was changed).
    /// A code that cannot be read must not break the list.
    /// </summary>
    public string? Unprotect(string? protectedCode)
    {
        if (string.IsNullOrEmpty(protectedCode))
            return null;

        try
        {
            var blob = Convert.FromBase64String(protectedCode);
            if (blob.Length < NonceSize + TagSize)
                return null;

            var plain = new byte[blob.Length - NonceSize - TagSize];
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(
                blob.AsSpan(0, NonceSize),
                blob.AsSpan(NonceSize + TagSize),
                blob.AsSpan(NonceSize, TagSize),
                plain
            );
            return System.Text.Encoding.UTF8.GetString(plain);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return null;
        }
    }

    private static bool TryReadKey(string value, out byte[] key)
    {
        key = new byte[KeySize];
        return Convert.TryFromBase64String(value, key, out var written) && written == KeySize;
    }
}
