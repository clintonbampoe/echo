using System.Security.Cryptography;
using System.Text;

namespace Echo.Auth.Invitations;

/// <summary>
/// Invitation codes people type by hand or read out: 12 characters from Crockford's base32 alphabet
/// (no I, L, O or U), 60 bits of randomness, shown in groups like K7QX-92MD-PW4T. Input is read
/// without regard to case, dashes or spaces, and the usual look-alikes (O for 0, I and L for 1).
/// </summary>
internal static class InvitationCode
{
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private const int Length = 12;

    internal static string Generate()
    {
        var code = new StringBuilder(Length);
        for (var i = 0; i < Length; i++)
            code.Append(Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]);

        return Format(code.ToString());
    }

    internal static string Format(string normalized) =>
        $"{normalized[..4]}-{normalized[4..8]}-{normalized[8..]}";

    /// <summary>The 12-character form that is hashed and compared, or null if the input cannot be a code.</summary>
    internal static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var normalized = new StringBuilder(Length);
        foreach (var c in input.ToUpperInvariant())
        {
            if (char.IsWhiteSpace(c) || c == '-')
                continue;

            var mapped = c switch
            {
                'O' => '0',
                'I' or 'L' => '1',
                _ => c,
            };
            if (!Alphabet.Contains(mapped))
                return null;

            normalized.Append(mapped);
        }

        return normalized.Length == Length ? normalized.ToString() : null;
    }
}
