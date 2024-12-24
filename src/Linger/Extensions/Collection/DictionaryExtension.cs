namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtension
{
    /// <summary>
    /// Gets the value associated with the specified key, or adds a new value created by the specified factory function if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate.</param>
    /// <param name="factory">The function to create a value if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the new value created by the factory function.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt;();
    /// int value = dict.GetOrAdd("key", k => 42);
    /// // value is 42
    /// </code>
    /// </example>
    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> factory)
    {
        if (dictionary.TryGetValue(key, out TValue? obj))
        {
            return obj;
        }

        return dictionary[key] = factory(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds a new value created by the specified factory function if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate.</param>
    /// <param name="factory">The function to create a value if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the new value created by the factory function.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt;();
    /// int value = dict.GetOrAdd("key", () => 42);
    /// // value is 42
    /// </code>
    /// </example>
    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> factory)
    {
        return dictionary.GetOrAdd(key, k => factory());
    }

    /// <summary>
    /// Adds a new value created by the specified add factory function if the key does not exist, or updates an existing value using the specified update factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate.</param>
    /// <param name="addFactory">The function to create a value if the key does not exist.</param>
    /// <param name="updateFactory">The function to update the value if the key exists.</param>
    /// <returns>The new or updated value associated with the specified key.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt;();
    /// int value = dict.AddOrUpdate("key", k => 42, (k, v) => v + 1);
    /// // value is 42
    /// value = dict.AddOrUpdate("key", k => 42, (k, v) => v + 1);
    /// // value is 43
    /// </code>
    /// </example>
    public static TValue AddOrUpdate<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> addFactory,
        Func<TKey, TValue, TValue> updateFactory)
    {
        if (dictionary.TryGetValue(key, out TValue? obj))
        {
            obj = updateFactory(key, obj);
        }
        else
        {
            obj = addFactory(key);
        }

        dictionary[key] = obj;
        return obj;
    }

    /// <summary>
    /// Adds a new value created by the specified add factory function if the key does not exist, or updates an existing value using the specified update factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to locate.</param>
    /// <param name="addFactory">The function to create a value if the key does not exist.</param>
    /// <param name="updateFactory">The function to update the value if the key exists.</param>
    /// <returns>The new or updated value associated with the specified key.</returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, int&gt;();
    /// int value = dict.AddOrUpdate("key", () => 42, v => v + 1);
    /// // value is 42
    /// value = dict.AddOrUpdate("key", () => 42, v => v + 1);
    /// // value is 43
    /// </code>
    /// </example>
    public static TValue AddOrUpdate<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> addFactory,
        Func<TValue, TValue> updateFactory)
    {
        return dictionary.AddOrUpdate(key, k => addFactory(), (k, v) => updateFactory(v));
    }
}
