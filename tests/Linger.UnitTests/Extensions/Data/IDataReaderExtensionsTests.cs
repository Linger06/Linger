namespace Linger.UnitTests.Extensions.Data;

public class IDataReaderExtensionsTests
{
    [Fact]
    public void ReaderToDataTable_ReturnsDataTableWithCorrectSchemaAndData()
    {
        var mockDataReader = new Mock<IDataReader>();
        mockDataReader.Setup(dr => dr.FieldCount).Returns(2);
        mockDataReader.Setup(dr => dr.GetName(0)).Returns("Id");
        mockDataReader.Setup(dr => dr.GetName(1)).Returns("Name");
        mockDataReader.Setup(dr => dr.GetFieldType(0)).Returns(typeof(int));
        mockDataReader.Setup(dr => dr.GetFieldType(1)).Returns(typeof(string));
        mockDataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(true)
            .Returns(false);
        mockDataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>())).Callback<object[]>(values =>
        {
            values[0] = 1;
            values[1] = "John";
        }).Returns(2);

        DataTable? result = mockDataReader.Object.ReaderToDataTable();

        Assert.Equal(2, result.Columns.Count);
        Assert.Equal("id", result.Columns[0].ColumnName);
        Assert.Equal("name", result.Columns[1].ColumnName);
        Assert.Equal(1, result.Rows[0]["id"]);
        Assert.Equal("John", result.Rows[0]["name"]);
    }

    [Fact]
    public void ReaderToList_ReturnsListOfObjects()
    {
        var mockDataReader = new Mock<IDataReader>();
        mockDataReader.Setup(dr => dr.FieldCount).Returns(2);
        mockDataReader.Setup(dr => dr.GetName(0)).Returns("Id");
        mockDataReader.Setup(dr => dr.GetName(1)).Returns("Name");
        mockDataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(true)
            .Returns(false);
        mockDataReader.Setup(dr => dr["Id"]).Returns(1);
        mockDataReader.Setup(dr => dr["Name"]).Returns("John");

        List<TestClass>? result = mockDataReader.Object.ReaderToList<TestClass>();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("John", result[0].Name);
    }

    [Fact]
    public void ReaderToModel_ReturnsSingleObject()
    {
        var mockDataReader = new Mock<IDataReader>();
        mockDataReader.Setup(dr => dr.FieldCount).Returns(2);
        mockDataReader.Setup(dr => dr.GetName(0)).Returns("Id");
        mockDataReader.Setup(dr => dr.GetName(1)).Returns("Name");
        mockDataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);
        mockDataReader.Setup(dr => dr["Id"]).Returns(1);
        mockDataReader.Setup(dr => dr["Name"]).Returns("John");

        TestClass? result = mockDataReader.Object.ReaderToModel<TestClass>();

        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public void ReaderToHashtable_ReturnsHashtableWithCorrectData()
    {
        var mockDataReader = new Mock<IDataReader>();
        mockDataReader.Setup(dr => dr.FieldCount).Returns(2);
        mockDataReader.Setup(dr => dr.GetName(0)).Returns("Id");
        mockDataReader.Setup(dr => dr.GetName(1)).Returns("Name");
        mockDataReader.SetupSequence(dr => dr.Read())
            .Returns(true)
            .Returns(false);
        mockDataReader.Setup(dr => dr["id"]).Returns(1);
        mockDataReader.Setup(dr => dr["name"]).Returns("John");

        Hashtable? result = mockDataReader.Object.ReaderToHashtable();

        Assert.Equal(1, result["id"]);
        Assert.Equal("John", result["name"]);
    }

    [Fact]
    public void HackType_ReturnsCorrectTypeForNullable()
    {
        var result = IDataReaderExtensions.HackType(null, typeof(int?));
        Assert.Null(result);

        result = IDataReaderExtensions.HackType(1, typeof(int?));
        Assert.Equal(1, result);
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}