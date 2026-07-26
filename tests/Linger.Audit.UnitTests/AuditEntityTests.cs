using Linger.Audit;

namespace Linger.Audit.UnitTests;

public class AuditEntityTests
{
    [Fact]
    public void FullAuditEntity_GenericTypeUsesCompleteInheritanceChain()
    {
        var entity = new TestFullAuditEntity();

        Assert.IsAssignableFrom<AuditEntity<int>>(entity);
        Assert.IsAssignableFrom<CreationAuditEntity<int>>(entity);
        Assert.IsAssignableFrom<BaseEntity<int>>(entity);
    }

    [Fact]
    public void FullAuditEntity_DefaultsToUnspecifiedDeletionStateAndUninitializedCreationTime()
    {
        var entity = new TestFullAuditEntity();

        Assert.Null(entity.IsDeleted);
        Assert.Equal(default, entity.CreationTime);
    }

    private sealed class TestFullAuditEntity : FullAuditEntity<int>
    {
    }
}
