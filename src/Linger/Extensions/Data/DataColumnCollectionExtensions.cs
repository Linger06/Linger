namespace Linger.Extensions.Data;

/// <summary>
/// Provides extension methods for the <see cref="DataColumnCollection"/> class.
/// </summary>
public static class DataColumnCollectionExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataColumnCollection"/>.
    /// </summary>
    /// <param name="dataColumnCollection">The <see cref="DataColumnCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataColumn}"/> to execute on each element.</param>
    public static void ForEach(this DataColumnCollection dataColumnCollection, Action<DataColumn> action)
    {
        foreach (DataColumn item in dataColumnCollection)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes the specified action on each element of the <see cref="DataColumnCollection"/>, providing the element's index.
    /// </summary>
    /// <param name="dataColumnCollection">The <see cref="DataColumnCollection"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{DataColumn, Int}"/> to execute on each element, with the element's index.</param>
    public static void ForEach(this DataColumnCollection dataColumnCollection, Action<DataColumn, int> action)
    {
        for (var i = 0; i < dataColumnCollection.Count; i++)
        {
            action(dataColumnCollection[i], i);
        }
    }
}
