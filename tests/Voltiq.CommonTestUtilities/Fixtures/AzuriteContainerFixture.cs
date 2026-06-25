using Testcontainers.Azurite;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace Voltiq.CommonTestUtilities.Fixtures;

public sealed class AzuriteContainerFixture(IMessageSink messageSink)
    : ContainerFixture<AzuriteBuilder, AzuriteContainer>(messageSink)
{
    protected override AzuriteBuilder Configure()
        => new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite")
            .WithCommand("--skipApiVersionCheck");
}
