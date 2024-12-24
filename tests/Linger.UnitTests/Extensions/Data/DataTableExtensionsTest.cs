namespace Linger.UnitTests.Extensions.Data;

public class DataTableExtensionsTest
{
    private readonly DataTable _dataTable;
    private readonly DataTable _dataTable2;

    public DataTableExtensionsTest()
    {
        _dataTable = new DataTable();
        _ = _dataTable.Columns.Add("column0", typeof(string));
        DataColumn dc2 = new("column1", typeof(DateTime));
        _dataTable.Columns.Add(dc2);

        _ = _dataTable.Rows.Add("John", DateTime.Now);
        _ = _dataTable.Rows.Add("Jane", DateTime.Now);
        _ = _dataTable.Rows.Add("Tony", DateTime.Now);
        _ = _dataTable.Rows.Add("Tiny", DateTime.Now);

        _dataTable2 = new DataTable();
        _ = _dataTable2.Columns.Add("column2", typeof(string));
        DataColumn dc3 = new("column3", typeof(DateTime));
        _dataTable2.Columns.Add(dc3);

        _ = _dataTable2.Rows.Add("John", DateTime.Now);
        _ = _dataTable2.Rows.Add("Jane", DateTime.Now);
        _ = _dataTable2.Rows.Add("Tony", DateTime.Now);
    }

    [Fact]
    public void LeftJoin()
    {
        //left join
        DataTable dt = _dataTable.Join(_dataTable2, new[] { _dataTable.Columns["column0"]! },
            new[] { _dataTable2.Columns["column2"]! }, true, false);
        DataTable df = dt.Find("column0 is null");
        DataTable dv = dt.Find("column2 = '' or column2 is null");
        Assert.Equal(0, df.Rows.Count);
        Assert.Equal("Tiny", dv.Rows[0]["column0"]);
        Assert.Equal(1, dv.Rows.Count);
    }

    [Fact]
    public void InnerJoin()
    {
        //inner join
        DataTable dt2 = _dataTable.Join(_dataTable2, new[] { _dataTable.Columns["column0"]! },
            new[] { _dataTable2.Columns["column2"]! }, false, false);
        DataTable df2 = dt2.Find("column0 is null");
        DataTable dv2 = dt2.Find("column2 is null");
        Assert.Equal(0, df2.Rows.Count);
        Assert.Equal(0, dv2.Rows.Count);
    }

    [Fact]
    public void RightJoin()
    {
        //right join
        DataTable dt3 = _dataTable.Join(_dataTable2, new[] { _dataTable.Columns["column0"]! },
            new[] { _dataTable2.Columns["column2"]! }, false, true);
        DataTable df3 = dt3.Find("column0 is null");
        DataTable dv3 = dt3.Find("column2 is null");
        Assert.Equal(0, df3.Rows.Count);
        Assert.Equal(0, dv3.Rows.Count);
    }

    [Fact]
    public void OutterJoin()
    {
        //Outter join
        _dataTable.Rows.RemoveAt(0);
        DataTable dt4 = _dataTable.Join(_dataTable2, new[] { _dataTable.Columns["column0"]! },
            new[] { _dataTable2.Columns["column2"]! }, true, true);
        DataTable df4 = dt4.Find("column0 is null");
        DataTable dv4 = dt4.Find("column2 is null");
        Assert.Equal(1, df4.Rows.Count);
        Assert.Equal("John", df4.Rows[0]["column2"]);
        Assert.Equal(1, dv4.Rows.Count);
        Assert.Equal("Tiny", dv4.Rows[0]["column0"]);
    }
}