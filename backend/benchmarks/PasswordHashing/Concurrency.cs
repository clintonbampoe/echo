using System.Diagnostics;

namespace PasswordHashing;

/// <summary>
/// Hashes per second when several hashes run at once, which BenchmarkDotNet (one at a time) cannot show. Run it pinned
/// to the cores the API gets, so the workers compete the way they would there: <c>taskset -c 0,1 dotnet run -c Release -- concurrency 10</c>.
/// </summary>
public static class Concurrency
{
    static readonly int[] _workers = [1, 2, 4];

    public static void Run(int seconds)
    {
        var bench = new HashBench();
        (string Name, Func<object> Hash)[] cases =
        [
            ("bcrypt cost 10", bench.Bcrypt_Cost10),
            ("argon2id 19 MiB t=2 native", bench.Argon2id_19MiB_2p_Native),
            ("argon2id 12 MiB t=3 native", bench.Argon2id_12MiB_3p_Native),
            ("argon2id 46 MiB t=1 native", bench.Argon2id_46MiB_1p_Native),
        ];

        Console.WriteLine(
            $"cores: {Environment.ProcessorCount} | profile: {Read("/sys/firmware/acpi/platform_profile")} | governor: {Read("/sys/devices/system/cpu/cpu0/cpufreq/scaling_governor")} | {seconds} s per row"
        );
        Console.WriteLine(
            $"{"algorithm", -28} {"workers", 7} {"hash/s", 8} {"x 1 worker", 10} {"cpu ms/hash", 11} {"peak MiB", 9}"
        );

        foreach (var (name, hash) in cases)
        {
            hash(); // loads libsodium and JITs, outside the timing
            double? one = null;
            foreach (var workers in _workers)
            {
                var (rate, cpuMs, peakMiB) = Measure(hash, workers, seconds);
                one ??= rate;
                Console.WriteLine(
                    $"{name, -28} {workers, 7} {rate, 8:F1} {rate / one.Value, 9:F2}x {cpuMs, 11:F0} {peakMiB, 9:F0}"
                );
            }
        }
    }

    static (double PerSecond, double CpuMs, double PeakMiB) Measure(
        Func<object> hash,
        int workers,
        int seconds
    )
    {
        GC.Collect();
        var process = Process.GetCurrentProcess();
        var baseline = process.WorkingSet64;
        var cpuStart = process.TotalProcessorTime;
        var peak = baseline;
        var count = 0L;

        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        var sampler = Task.Run(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                process.Refresh();
                peak = Math.Max(peak, process.WorkingSet64);
                Thread.Sleep(50);
            }
        });

        var started = Stopwatch.GetTimestamp();
        var tasks = Enumerable
            .Range(0, workers)
            .Select(_ =>
                Task.Factory.StartNew(
                    () =>
                    {
                        while (!stop.IsCancellationRequested)
                        {
                            hash();
                            Interlocked.Increment(ref count);
                        }
                    },
                    TaskCreationOptions.LongRunning
                )
            )
            .ToArray();
        Task.WaitAll(tasks);
        var elapsed = Stopwatch.GetElapsedTime(started).TotalSeconds;
        sampler.Wait();

        process.Refresh();
        var cpuMs = (process.TotalProcessorTime - cpuStart).TotalMilliseconds / count;
        return (count / elapsed, cpuMs, (peak - baseline) / 1048576.0);
    }

    static string Read(string path) => File.Exists(path) ? File.ReadAllText(path).Trim() : "n/a";
}
