using System.ComponentModel.DataAnnotations;

namespace Echo.Shared.Options.Hashing;

/// <summary>
/// How many password hashes may run at once, and how long a line of them may get (see HashGate). Set in
/// configuration under "Hashing", or with the environment variables Hashing__MaxConcurrent and
/// Hashing__MaxQueueSeconds.
/// </summary>
public class HashingOptions
{
    public const string Section = "Hashing";

    /// <summary>
    /// Hashes running at once. Left out, it is one less than the cores the API may use (at least 1), so one core is
    /// always free for the people who are already signed in. More means a crowd signs in faster and everyone else waits
    /// longer; each hash takes about 175 ms of one core.
    /// </summary>
    [Range(1, 256, ErrorMessage = "Hashing:MaxConcurrent must be between 1 and 256.")]
    public int? MaxConcurrent { get; init; }

    /// <summary>
    /// Seconds of hashing that may be waiting. A caller who would wait longer is told to try again, with a time.
    /// </summary>
    [Range(1, 300, ErrorMessage = "Hashing:MaxQueueSeconds must be between 1 and 300.")]
    public int MaxQueueSeconds { get; init; } = 10;
}
