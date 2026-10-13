namespace Echo.Shared.Utilities;

public readonly record struct DateWindow(DateOnly From, DateOnly To, bool IsDefault);
