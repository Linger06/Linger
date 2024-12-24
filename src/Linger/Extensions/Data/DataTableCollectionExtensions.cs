namespace Linger.Extensions.Data;

/// <summary>
/// Extensions for <see cref="DataTableCollection"/>.
/// </summary>
public static class DataTableCollectionExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataTableCollection"/>.
    /// </summary>
    /// <param name="dataTableCollection">The <see cref="DataTableCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataTable}"/> to execute on each element.</param>
    /// <remarks>
    /// This method allows you to perform a specific action on each <see cref="DataTable"/> in the collection.
    /// </remarks>
    /// <example>
    /// <code>
    /// DataSet dataSet = GetDataSet();
    /// dataSet.Tables.ForEach(table => Console.WriteLine(table.TableName));
    /// </code>
    /// </example>
    public static void ForEach(this DataTableCollection dataTableCollection, Action<DataTable> action)
    {
        foreach (DataTable item in dataTableCollection)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataTableCollection"/>, providing the element's index.
    /// </summary>
    /// <param name="dataTableCollection">The <see cref="DataTableCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataTable, Int}"/> to execute on each element, with the element's index.</param>
    /// <remarks>
    /// This method allows you to perform a specific action on each <see cref="DataTable"/> in the collection, with access to the table's index.
    /// </remarks>
    /// <example>
    /// <code>
    /// DataSet dataSet = GetDataSet();
    /// dataSet.Tables.ForEach((table, index) => Console.WriteLine($"Table {index}: {table.TableName}"));
    /// </code>
    /// </example>
    public static void ForEach(this DataTableCollection dataTableCollection, Action<DataTable, int> action)
    {
        for (var i = 0; i < dataTableCollection.Count; i++)
        {
            action(dataTableCollection[i], i);
        }
    }
}
