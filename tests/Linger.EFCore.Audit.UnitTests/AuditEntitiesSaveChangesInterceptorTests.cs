using Linger.Audit;
using Linger.Audit.Contracts;
using Linger.EFCore.Audit;
using Linger.EFCore.Audit.Interceptors;
using Linger.Extensions.Core;
using Microsoft.EntityFrameworkCore;
using Moq;

public class AuditEntitiesSaveChangesInterceptorTests
{
    private readonly Mock<IAuditUserProvider> _mockUserProvider;
    private readonly TestDbContext _dbContext;

    public AuditEntitiesSaveChangesInterceptorTests()
    {
        _mockUserProvider = new Mock<IAuditUserProvider>();
        _mockUserProvider.Setup(x => x.UserName).Returns("TestUser");
        _mockUserProvider.Setup(x => x.GetUser()).Returns("TestUser");

        var interceptor = new AuditEntitiesSaveChangesInterceptor(_mockUserProvider.Object);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        _dbContext = new TestDbContext(options);
    }

    [Fact]
    public async Task SavingChanges_WhenAddingEntity_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntity { Name = "Test" };

        // Act
        await _dbContext.TestEntities.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails.SingleAsync();
        Assert.Equal(AuditType.Added, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal("TestAuditEntity", auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.NewValues);
        Assert.Contains("Name", auditEntry.NewValues.Keys);
        Assert.Equal("Test", auditEntry.NewValues["Name"]);
    }

    [Fact]
    public async Task SavingChanges_WhenModifyingEntity_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntity { Name = "Test" };
        await _dbContext.TestEntities.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        // Act
        testEntity.Name = "Updated";
        _dbContext.TestEntities.Update(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails
            .OrderByDescending(x => x.TimeStamp)
            .FirstAsync();

        var entity = await _dbContext.TestEntities.FindAsync(testEntity.Id);

        Assert.Equal("Updated", entity.Name);
        Assert.Equal(AuditType.Modified, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal("TestAuditEntity", auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.OldValues);
        Assert.NotNull(auditEntry.NewValues);
        Assert.Contains("Name", auditEntry.AffectedColumns!);
        Assert.Equal("Test", auditEntry.OldValues["Name"]);
        Assert.Equal("Updated", auditEntry.NewValues["Name"]);
    }

    [Fact]
    public async Task SavingChanges_WhenDeletingEntity_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntity { Name = "Test" };
        await _dbContext.TestEntities.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        _dbContext.TestEntities.Remove(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails
            .OrderByDescending(x => x.TimeStamp)
            .FirstAsync();

        Assert.Equal(AuditType.Modified, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal("TestAuditEntity", auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.OldValues);
        Assert.Contains("IsDeleted", auditEntry.OldValues.Keys);
        Assert.False(auditEntry.OldValues["IsDeleted"].ToBool());
        Assert.True(auditEntry.NewValues["IsDeleted"].ToBool());
    }

    [Fact]
    public async Task SavingChanges_WhenAddingEntityWithoutSoftDelete_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntityWithoutSoftDelete { Name = "Test" };

        // Act
        await _dbContext.TestEntitiesWithoutSoftDelete.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails.SingleAsync();
        Assert.Equal(AuditType.Added, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal(nameof(TestAuditEntityWithoutSoftDelete), auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.NewValues);
        Assert.Contains("Name", auditEntry.NewValues.Keys);
        Assert.Equal("Test", auditEntry.NewValues["Name"]);
    }

    [Fact]
    public async Task SavingChanges_WhenModifyingEntityWithoutSoftDelete_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntityWithoutSoftDelete { Name = "Test" };
        await _dbContext.TestEntitiesWithoutSoftDelete.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        // Act
        testEntity.Name = "Updated";
        _dbContext.TestEntitiesWithoutSoftDelete.Update(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails
            .OrderByDescending(x => x.TimeStamp)
            .FirstAsync();

        Assert.Equal(AuditType.Modified, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal(nameof(TestAuditEntityWithoutSoftDelete), auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.OldValues);
        Assert.NotNull(auditEntry.NewValues);
        Assert.Contains("Name", auditEntry.AffectedColumns!);
        Assert.Equal("Test", auditEntry.OldValues["Name"]);
        Assert.Equal("Updated", auditEntry.NewValues["Name"]);
    }

    [Fact]
    public async Task SavingChanges_WhenDeletingEntityWithoutSoftDelete_ShouldCreateAuditTrail()
    {
        // Arrange
        var testEntity = new TestAuditEntityWithoutSoftDelete { Name = "Test" };
        await _dbContext.TestEntitiesWithoutSoftDelete.AddAsync(testEntity);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        // Act
        _dbContext.TestEntitiesWithoutSoftDelete.Remove(testEntity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntry = await _dbContext.AuditTrails
            .OrderByDescending(x => x.TimeStamp)
            .FirstAsync();

        Assert.Equal(AuditType.Deleted, auditEntry.AuditType);
        Assert.Equal("TestUser", auditEntry.Username);
        Assert.Equal(nameof(TestAuditEntityWithoutSoftDelete), auditEntry.EntityName);
        Assert.Equal(testEntity.Id.ToString(), auditEntry.EntityId);
        Assert.NotNull(auditEntry.OldValues);
        Assert.Contains("Name", auditEntry.OldValues.Keys);
        Assert.Equal("Test", auditEntry.OldValues["Name"]);
    }

}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestAuditEntity> TestEntities { get; set; } = null!;
    public DbSet<TestAuditEntityWithoutSoftDelete> TestEntitiesWithoutSoftDelete { get; set; } = null!;

    public DbSet<AuditTrailEntry> AuditTrails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyAudit();
    }
}

public class TestAuditEntity : FullAuditEntity<int>
{
    public string Name { get; set; } = null!;
}

public class TestAuditEntityWithoutSoftDelete : AuditEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}