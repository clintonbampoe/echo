namespace Echo.Auth.Validation;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static bool IsValid(string? password, out string? error)
    {
        if (string.IsNullOrEmpty(password))
        {
            error = "Password is required";
            return false;
        }

        if (password.Length < MinLength)
        {
            error = $"Password must be at least {MinLength} characters.";
            return false;
        }

        if (password.Length > MaxLength)
        {
            error = $"Password must be at most {MaxLength} characters.";
            return false;
        }

        error = null;
        return true;
    }
}
