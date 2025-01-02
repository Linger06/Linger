using Microsoft.EntityFrameworkCore;

namespace Linger.EFCore.UnitTests;

public class GlobalQueryFiltersTests
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }

    public class TestEntity : ISoftDelete
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class TestEntity2
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
        public DbSet<TestEntity2> TestEntities2 { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyGlobalFilters<ISoftDelete>(x => !x.IsDeleted);
            modelBuilder.ApplyGlobalFilters("IsDeleted", false);
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
            context.TestEntities.Add(new TestEntity { Id = 1, IsDeleted = false });
            context.TestEntities.Add(new TestEntity { Id = 2, IsDeleted = true });
            context.SaveChanges();

            // Act
            var result = context.TestEntities.ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
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
            context.TestEntities2.Add(new TestEntity2 { Id = 1, IsDeleted = false });
            context.TestEntities2.Add(new TestEntity2 { Id = 2, IsDeleted = true });
            context.SaveChanges();

            // Act
            var result = context.TestEntities2.ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
    }
}
