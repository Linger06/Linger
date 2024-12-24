using System.Collections.Specialized;
using Linger.Extensions.Core;

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for <see cref="NameValueCollection"/>.
/// </summary>
/// <value>Static class containing extension methods for NameValueCollection.</value>
public static class NameValueCollectionExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the current collection.
    /// </summary>
    /// <param name="collection">The current collection.</param>
    /// <param name="action">
    /// The <see cref="Action{T1, T2}"/> delegate to perform on each element of the <see cref="NameValueCollection"/>.
    /// </param>
    /// <example>
    /// <code>
    /// var collection = new NameValueCollection();
    /// collection.Add("key1", "value1");
    /// collection.Add("key2", "value2");
    /// collection.ForEach((key, value) => Console.WriteLine($"{key}: {value}"));
    /// // Output:
    /// // key1: value1
    /// // key2: value2
    /// </code>
    /// </example>
    public static void ForEach(this NameValueCollection collection, Action<string, string> action)
    {
        if (collection.Count <= 0)
        {
            return;
        }

        var keys = collection.AllKeys;
        if (keys.IsNotNull())
        {
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var value = collection[i];
                if (key.IsNotNull() && value.IsNotNull())
                {
                    action(key, value);
                }
            }
        }
    }

    /// <summary>
    /// Executes the specified action on each element of the current collection.
    /// </summary>
    /// <param name="collection">The current collection.</param>
    /// <param name="action">
    /// The <see cref="Action{T1, T2, T3}"/> delegate to perform on each element of the <see cref="NameValueCollection"/>.
    /// </param>
    /// <example>
    /// <code>
    /// var collection = new NameValueCollection();
    /// collection.Add("key1", "value1");
    /// collection.Add("key2", "value2");
    /// collection.ForEach((key, value, index) => Console.WriteLine($"{index}: {key} - {value}"));
    /// // Output:
    /// // 0: key1 - value1
    /// // 1: key2 - value2
    /// </code>
    /// </example>
    public static void ForEach(this NameValueCollection collection, Action<string, string, int> action)
    {
        if (collection.Count <= 0)
        {
            return;
        }

        var keys = collection.AllKeys;
        if (keys.IsNotNull())
        {
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var value = collection[i];
                if (key.IsNotNull() && value.IsNotNull())
                {
                    action(key, value, i);
                }
            }
        }
    }
}
