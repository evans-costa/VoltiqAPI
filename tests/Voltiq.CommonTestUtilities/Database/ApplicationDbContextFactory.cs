using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Infrastructure.Persistence;

namespace Voltiq.CommonTestUtilities.Database;

public static class ApplicationDbContextFactory
{
    public static ApplicationDbContext Create(string connectionString, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(userId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new ApplicationDbContext(options, currentUser.Object);
    }
}
