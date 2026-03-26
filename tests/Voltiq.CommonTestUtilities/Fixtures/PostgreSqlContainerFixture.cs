using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace Voltiq.CommonTestUtilities.Fixtures;

public sealed class PostgreSqlContainerFixture(IMessageSink messageSink)
    : ContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)
{
    protected override PostgreSqlBuilder Configure()
        => new PostgreSqlBuilder("postgres:16-alpine");
}
