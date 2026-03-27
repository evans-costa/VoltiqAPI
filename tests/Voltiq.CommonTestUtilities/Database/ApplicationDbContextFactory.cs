using Microsoft.EntityFrameworkCore;
using Moq;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Interceptors;

namespace Voltiq.CommonTestUtilities.Database;

public static class ApplicationDbContextFactory
{
    public static ApplicationDbContext Create(string connectionString, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(userId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;

        return new ApplicationDbContext(options, currentUser.Object);
    }
}
