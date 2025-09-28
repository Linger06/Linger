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
        Assert.Equal(11, dataTable.Columns.Count);
    }

    [Fact]
    public void ColumnCountIsCorrect()
    {
        var dataTable = _class.ToDataTable();
        Assert.Equal(14, dataTable.Columns.Count);
    }

    [Fact]
    public void ColumnNamesAreCorrect()
    {
        var dataTable = _class.ToDataTable();
        Assert.Equal("Int", dataTable.Columns[0].ColumnName);
    }

    [Fact]
    public void HandlesBinary()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Binary, dataRow[7]);
    }

    [Fact]
    public void HandlesBoolean()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Boolean, dataRow[8]);
    }

    [Fact]
    public void HandlesDateTime()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().DateTime, dataRow[5]);
    }

    [Fact]
    public void HandlesDecimal()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Decimal, dataRow[11]);
    }

    [Fact]
    public void HandlesDouble()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Double, dataRow[13]);
    }

    [Fact]
    public void HandlesGuid()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Guid, dataRow[3]);
    }

    [Fact]
    public void HandlesInt()
    {
        var dataTable = _class.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(1, dataRow[0]);
    }

    [Fact]
    public void HandlesInt16()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Int16, dataRow[9]);
    }

    [Fact]
    public void HandlesInt64()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Int64, dataRow[10]);
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
        Assert.Equal(DBNull.Value, dataRow[4]);
    }

    [Fact]
    public void HandlesNullableInt()
    {
        var dataTable = _class.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(DBNull.Value, dataRow[1]);
    }

    [Fact]
    public void HandlesSingle()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().Single, dataRow[12]);
    }

    [Fact]
    public void HandlesString()
    {
        IEnumerable<TestClass> testEnumerable = _class;
        var dataTable = testEnumerable.ToDataTable();
        DataRow dataRow = dataTable.Rows[0];
        Assert.Equal(testEnumerable.First().String, dataRow[2]);
    }

    [Fact]
    public void HasCorrectNumberOfRows()
    {
        var dataTable = _class.ToDataTable();
        Assert.Equal(2, dataTable.Rows.Count);
    }
}