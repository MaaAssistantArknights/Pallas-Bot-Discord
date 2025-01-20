using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;

namespace PallasBot.Aspire.ServiceDefaults.Internal;

public class InternalResourceDetector : IResourceDetector
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public InternalResourceDetector(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public Resource Detect()
    {
        var svcName = _configuration["OTEL_SERVICE_NAME"] ?? _hostEnvironment.ApplicationName;
        var svcNamespace = _hostEnvironment.EnvironmentName;

        var assembly = Assembly.GetExecutingAssembly();
        var svcVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";

        return ResourceBuilder.CreateEmpty()
            .AddService(svcName, svcNamespace, svcVersion)
            .AddTelemetrySdk()
            .Build();
    }
}
