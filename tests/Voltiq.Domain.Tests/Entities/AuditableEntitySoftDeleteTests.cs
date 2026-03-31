using Shouldly;
using Voltiq.Domain.Entities;

namespace Voltiq.Domain.Tests.Entities;

// Concrete implementation for testing the abstract AuditableEntity
file sealed class TestAuditableEntity : AuditableEntity;

public class AuditableEntitySoftDeleteTests
{
    [Fact]
    public void NewEntity_ShouldNotBeDeleted()
    {
        var entity = new TestAuditableEntity();

        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public void SoftDelete_ShouldMarkEntityAsDeleted()
    {
        var entity = new TestAuditableEntity();

        entity.SoftDelete();

        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new TestAuditableEntity();

        entity.SoftDelete();

        entity.DeletedAt.ShouldNotBeNull();
        entity.DeletedAt!.Value.ShouldBeGreaterThanOrEqualTo(before);
        entity.DeletedAt!.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void SoftDelete_CalledTwice_ShouldNotChangeDates()
    {
        var entity = new TestAuditableEntity();

        entity.SoftDelete();
        var firstDeletedAt = entity.DeletedAt;

        entity.SoftDelete();

        entity.DeletedAt.ShouldBe(firstDeletedAt);
    }
}
