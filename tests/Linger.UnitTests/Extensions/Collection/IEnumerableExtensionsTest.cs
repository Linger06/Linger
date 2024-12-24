namespace Linger.UnitTests.Extensions.Collection;

public class IEnumerableExtensionsTest
{
    private readonly IEnumerable<TestClass> _class = new[]
    {
        new TestClass
        {
            Int = 1,
            NullableInt = null,
            String = Guid.NewGuid().ToString(),
            Guid = Guid.NewGuid(),
            NullableGuid = null,
            DateTime = new DateTime(2020, 1, 1),
            NullableDateTime = null,
            Binary = Guid.NewGuid().ToByteArray(),
            Boolean = true,
            Int16 = 2,
            Int64 = 3,
            Decimal = new decimal(1.1),
            Single = 4,
            Double = 5
        },
        new TestClass
        {
            Int = 11,
            NullableInt = null,
            String = Guid.NewGuid().ToString(),
            Guid = Guid.NewGuid(),
            NullableGuid = null,
            DateTime = new DateTime(2021, 2, 2),
            NullableDateTime = null,
            Binary = Guid.NewGuid().ToByteArray(),
            Boolean = true,
            Int16 = 21,
            Int64 = 31,
            Decimal = new decimal(11.11),
            Single = 41,
            Double = 51
        }
    };

    [Fact]
    public void CheckAnonymousTypes()
    {
        var class2 = new[]
        {
            new
            {
                Int = 1,
                String = Guid.NewGuid().ToString(),
                Guid = Guid.NewGuid(),
                DateTime = new DateTime(2020, 1, 1),
                Binary = Guid.NewGuid().ToByteArray(),
                Boolean = true,
                Int16 = 2,
                Int64 = 3,
                Decimal = new decimal(1.1),
                Single = 4,
                Double = 5
            },
            new
            {
                Int = 11,
                String = Guid.NewGuid().ToString(),
                Guid = Guid.NewGuid(),
                DateTime = new DateTime(2021, 2, 2),
                Binary = Guid.NewGuid().ToByteArray(),
                Boolean = true,
                Int16 = 21,
                Int64 = 31,
                Decimal = new decimal(11.11),
                Single = 41,
                Double = 51
            }
        };
        var dataTable = class2.ToDataTable();
        _ = dataTable.Columns.Count.Should().Be(11);
    }

    [Fact]
    public void ColumnCountIsCorrect()
    {
        var dataTable = _class.ToDataTable();
        _ = dataTable.Columns.Count.Should().Be(14);
    }

    [Fact]
    public void ColumnNamesAreCorrect()
    {
        var dataTable = _class.ToDataTable();
        _ = dataTable.Columns[0].ColumnName.Should().Be("Int");
    }

    [Fact]
    public void HandlesBinary()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[7].Should().Be(testEnumerable.First().Binary);
    }

    [Fact]
    public void HandlesBoolean()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[8].Should().Be(testEnumerable.First().Boolean);
    }

    [Fact]
    public void HandlesDateTime()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[5].Should().Be(testEnumerable.First().DateTime);
    }

    [Fact]
    public void HandlesDecimal()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[11].Should().Be(testEnumerable.First().Decimal);
    }

    [Fact]
    public void HandlesDouble()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[13].Should().Be(testEnumerable.First().Double);
    }

    [Fact]
    public void HandlesGuid()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[3].Should().Be(testEnumerable.First().Guid);
    }

    [Fact]
    public void HandlesInt()
    {
        var dataTable = _class.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[0].Should().Be(1);
    }

    [Fact]
    public void HandlesInt16()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[9].Should().Be(testEnumerable.First().Int16);
    }

    [Fact]
    public void HandlesInt64()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[10].Should().Be(testEnumerable.First().Int64);
    }

    [Fact]
    public void HandlesNullableDateTime()
    {
        //Arrange

        //Act
        var dataTable = _class.ToDataTable();

        //Assert
        Assert.Equal(dataTable.Rows[0][6], DBNull.Value);
    }

    [Fact]
    public void HandlesNullableGuid()
    {
        var dataTable = _class.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[4].Should().Be(DBNull.Value);
    }

    [Fact]
    public void HandlesNullableInt()
    {
        var dataTable = _class.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[1].Should().Be(DBNull.Value);
    }

    [Fact]
    public void HandlesSingle()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[12].Should().Be(testEnumerable.First().Single);
    }

    [Fact]
    public void HandlesString()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        _ = dataRow[2].Should().Be(testEnumerable.First().String);
    }

    [Fact]
    public void HasCorrectNumberOfRows()
    {
        var dataTable = _class.ToDataTable();
        _ = dataTable.Rows.Count.Should().Be(2);
    }
}