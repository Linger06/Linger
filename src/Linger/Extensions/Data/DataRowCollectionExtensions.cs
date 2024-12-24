namespace Linger.Extensions.Data;

/// <summary>
/// Provides extension methods for the <see cref="DataRowCollection"/> class.
/// </summary>
public static class DataRowCollectionExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataRowCollection"/>.
    /// </summary>
    /// <param name="dataRowCollection">The <see cref="DataRowCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataRow}"/> to execute on each element.</param>
    /// <remarks>
    /// This method allows you to perform a specific action on each <see cref="DataRow"/> in the collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// table.Rows.ForEach(row => Console.WriteLine(row["ColumnName"]));
    /// </code>
    /// </example>
    public static void ForEach(this DataRowCollection dataRowCollection, Action<DataRow> action)
    {
        foreach (DataRow item in dataRowCollection)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataRowCollection"/>, providing the element's index.
    /// <para>Example: <example>
    /// <code>
    /// DataTable table = GetDataTable();
    /// table.Rows.ForEach((row, index) => Console.WriteLine($"Row {index}: {row["ColumnName"]}"));
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    /// <param name="dataRowCollection">The <see cref="DataRowCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataRow, Int}"/> to execute on each element, with the element's index.</param>
    /// <remarks>
    /// This method allows you to perform a specific action on each <see cref="DataRow"/> in the collection, with access to the row's index.
    /// </remarks>
    public static void ForEach(this DataRowCollection dataRowCollection, Action<DataRow, int> action)
    {
        for (var i = 0; i < dataRowCollection.Count; i++)
        {
            action(dataRowCollection[i], i);
        }
    }
}