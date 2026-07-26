using Microsoft.EntityFrameworkCore;

namespace Linger.EFCore.UnitTests;

public class GlobalQueryFiltersTests
{
    public interface ISoftDelete
    {
        bool? IsDeleted { get; set; }
    }

    public class TestEntity : ISoftDelete
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class TestEntity2
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class TestDbContext : DbContext
    {
        private readonly int _tenantId;

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
        public DbSet<TestEntity2> TestEntities2 { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options, int tenantId = 1) : base(options)
        {
            _tenantId = tenantId;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyGlobalFilters<ISoftDelete>(x => x.IsDeleted != true);
            modelBuilder.ApplyGlobalFilters("TenantId", () => _tenantId);
        }
    }

    [Fact]
    public void ApplyGlobalFilters_Interface_ShouldFilterDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb1")
            .Options;

        using (var context = new TestDbContext(options))
        {
            context.TestEntities.Add(new TestEntity { Id = 1, TenantId = 1, IsDeleted = false });
            context.TestEntities.Add(new TestEntity { Id = 2, TenantId = 1, IsDeleted = true });
            context.TestEntities.Add(new TestEntity { Id = 3, TenantId = 2, IsDeleted = false });
            context.TestEntities.Add(new TestEntity { Id = 4, TenantId = 1, IsDeleted = null });
            context.SaveChanges();

            // Act
            var result = context.TestEntities.ToList();

            // Assert
            Assert.Equal([1, 4], result.Select(entity => entity.Id).Order());
        }
    }

    [Fact]
    public void ApplyGlobalFilters_Property_ShouldFilterDeletedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb2")
            .Options;

        using (var context = new TestDbContext(options))
        {
            context.TestEntities2.Add(new TestEntity2 { Id = 1, TenantId = 1, IsDeleted = false });
            context.TestEntities2.Add(new TestEntity2 { Id = 2, TenantId = 2, IsDeleted = false });
            context.SaveChanges();

            // Act
            var result = context.TestEntities2.ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
    }

    [Fact]
    public void ApplyGlobalFilters_PropertyValueUsesCurrentContextInstance()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        using (var context = new TestDbContext(options, tenantId: 1))
        {
            context.TestEntities2.Add(new TestEntity2 { Id = 1, TenantId = 1 });
            context.TestEntities2.Add(new TestEntity2 { Id = 2, TenantId = 2 });
            context.SaveChanges();
        }

        using var tenantTwoContext = new TestDbContext(options, tenantId: 2);
        var result = tenantTwoContext.TestEntities2.Single();

        Assert.Equal(2, result.Id);
    }
}
