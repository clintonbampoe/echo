using Echo.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class FrontendClientConfigurationExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenBlank_Throws(string? baseUrl)
    {
        var configuration = CreateConfiguration(baseUrl);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Production")));

        Assert.Contains("must be set", exception.Message);
    }

    [Theory]
    [InlineData("localhost:5173")]
    [InlineData("not-a-url")]
    public void Validate_WhenNotAbsolute_ThrowsEvenInDevelopment(string baseUrl)
    {
        var configuration = CreateConfiguration(baseUrl);

        Assert.Throws<InvalidOperationException>(() =>
            configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Development")));
    }

    [Fact]
    public void Validate_HttpInDevelopment_ReturnsUrl()
    {
        var configuration = CreateConfiguration("http://localhost:5173");

        var result = configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Development"));

        Assert.Equal("http://localhost:5173", result);
    }

    [Fact]
    public void Validate_HttpInProduction_Throws()
    {
        var configuration = CreateConfiguration("http://localhost:5173");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Production")));

        Assert.Contains("https", exception.Message);
    }

    [Fact]
    public void Validate_HttpsInProduction_ReturnsUrl()
    {
        var configuration = CreateConfiguration("https://church.example.com");

        var result = configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Production"));

        Assert.Equal("https://church.example.com", result);
    }

    [Fact]
    public void Validate_TrailingSlashIsTrimmed()
    {
        var configuration = CreateConfiguration("https://church.example.com/");

        var result = configuration.ValidateFrontendClientBaseUrl(new TestHostEnvironment("Production"));

        Assert.Equal("https://church.example.com", result);
    }

    private static IConfiguration CreateConfiguration(string? baseUrl)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["FrontendClient:BaseUrl"] = baseUrl,
                }
            )
            .Build();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Echo.Api.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Directory.GetCurrentDirectory());
    }
}
