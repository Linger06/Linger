using Microsoft.EntityFrameworkCore;

namespace Linger.EFCore.UnitTests;

public class PropertyBuilderExtensionsTests
{
    public class TestEntity
    {
        public int Id { get; set; }
        public JsonData? JsonProperty { get; set; }
        public IEnumerable<string>? StringEnumerable { get; set; }
        public ICollection<string>? StringCollection { get; set; }
        public List<string>? StringList { get; set; }
        public string[]? StringArray { get; set; }
    }

    public class JsonData
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(builder =>
            {
                builder.Property(e => e.JsonProperty)
                    .HasJsonConversion();

                builder.Property(e => e.StringEnumerable)
                    .HasStringCollectionConversion();

                builder.Property(e => e.StringCollection)
                    .HasStringCollectionConversion();

                builder.Property(e => e.StringList)
                    .HasStringCollectionConversion();

                builder.Property(e => e.StringArray)
                    .HasStringCollectionConversion();
            });
        }
    }

    [Fact]
    public async Task HasJsonConversion_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "JsonConversionTest")
            .Options;

        var testData = new JsonData { Name = "Test", Age = 25 };
        var entity = new TestEntity { JsonProperty = testData };

        // Act & Assert
        await using (var context = new TestDbContext(options))
        {
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = new TestDbContext(options))
        {
            var loadedEntity = await context.TestEntities.SingleAsync();
            Assert.NotNull(loadedEntity.JsonProperty);
            Assert.Equal(testData.Name, loadedEntity.JsonProperty.Name);
            Assert.Equal(testData.Age, loadedEntity.JsonProperty.Age);
        }
    }

    [Fact]
    public async Task HasArrayConversion_WithIEnumerable_ShouldConvertCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "EnumerableTest")
            .Options;

        var strings = new[] { "one", "two", "three" };
        var entity = new TestEntity { StringEnumerable = strings };

        // Act & Assert
        await using (var context = new TestDbContext(options))
        {
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = new TestDbContext(options))
        {
            var loadedEntity = await context.TestEntities.SingleAsync();
            Assert.NotNull(loadedEntity.StringEnumerable);
            Assert.Equal(strings, loadedEntity.StringEnumerable);
        }
    }

    [Fact]
    public async Task HasArrayConversion_WithICollection_ShouldConvertCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "CollectionTest")
            .Options;

        var strings = new List<string> { "one", "two", "three" };
        var entity = new TestEntity { StringCollection = strings };

        // Act & Assert
        await using (var context = new TestDbContext(options))
        {
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = new TestDbContext(options))
        {
            var loadedEntity = await context.TestEntities.SingleAsync();
            Assert.NotNull(loadedEntity.StringCollection);
            Assert.Equal(strings, loadedEntity.StringCollection);
        }
    }

    [Fact]
    public async Task HasStringCollectionConversion_ShouldConvertCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: "StringCollectionTest")
            .Options;

        var strings = new List<string> { "one", "two", "three" };
        var entity = new TestEntity { StringList = strings };

        // Act & Assert
        await using (var context = new TestDbContext(options))
        {
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = new TestDbContext(options))
        {
            var loadedEntity = await context.TestEntities.SingleAsync();
            Assert.NotNull(loadedEntity.StringList);
            Assert.Equal(strings, loadedEntity.StringList);
        }
    }

    [Theory]
    [InlineData(";")]
    [InlineData("|")]
    [InlineData(",")]
    public async Task HasArrayConversion_WithDifferentSeparators_ShouldWorkCorrectly(string separator)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"SeparatorTest_{separator}")
            .Options;

        var strings = new[] { "one", "two", "three" };
        var entity = new TestEntity { StringEnumerable = strings };

        // Act & Assert
        await using (var context = new TestDbContext(options))
        {
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = new TestDbContext(options))
        {
            var loadedEntity = await context.TestEntities.SingleAsync();
            Assert.NotNull(loadedEntity.StringEnumerable);
            Assert.Equal(strings, loadedEntity.StringEnumerable);
        }
    }
}
