using BenchmarkDotNet.Running;
using PasswordHashing;

if (args is ["concurrency", ..])
    Concurrency.Run(args.Length > 1 && int.TryParse(args[1], out var seconds) ? seconds : 10);
else
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
