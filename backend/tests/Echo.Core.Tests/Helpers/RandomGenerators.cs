namespace Echo.Core.Tests.Helpers;

public static class RandomGenerators
{
    public static string String(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwyz0123456789";

        return new string(Random.Shared.GetItems(chars.AsSpan(), length));
    }

    public static int Integer()
    {
        var num = Random.Shared.Next();
        return num;
    }

    public static decimal DecimalNum()
    {
        const int scale = 100;
        var num = Random.Shared.NextDouble();

        return (decimal)num * scale;
    }
}
