namespace Linger;

/// <summary>
/// Represents a list of parameters.
/// </summary>
[Serializable]
public class ParameterList : IEnumerable<KeyValuePair<string, object>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> class.
    /// </summary>
    public ParameterList()
    {
        Parameters = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> class with a single key-value pair.
    /// </summary>
    /// <param name="key">The key of the parameter.</param>
    /// <param name="data">The value of the parameter.</param>
    public ParameterList(string key, object data) : this()
    {
        Add(key, data);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterList"/> class with multiple key-value pairs.
    /// </summary>
    /// <param name="keyArray">The array of keys.</param>
    /// <param name="dataArray">The array of values.</param>
    /// <exception cref="ArgumentException">Thrown when the lengths of the key and data arrays do not match.</exception>
    public ParameterList(string[] keyArray, object[] dataArray) : this()
    {
        if (keyArray.Length != dataArray.Length)
        {
            throw new ArgumentException("Key array length does not match data array length.");
        }

        for (var i = 0; i < keyArray.Length; i++)
        {
            Add(keyArray[i], dataArray[i]);
        }
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    /// Returns an enumerator that iterates through the parameters.
    /// </summary>
    /// <returns>An enumerator for the parameters.</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return Parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds a key-value pair to the parameter list.
    /// </summary>
    /// <param name="key">The key of the parameter.</param>
    /// <param name="data">The value of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the key already exists in the parameter list.</exception>
    public void Add(string key, object data)
    {
        if (Parameters.ContainsKey(key))
        {
            throw new ArgumentException($"The key '{key}' already exists in the parameter list.");
        }

        Parameters.Add(key, data);
    }

    /// <summary>
    /// Gets the value of the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the parameter.</param>
    /// <returns>The value of the parameter.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key does not exist in the parameter list.</exception>
    public T Get<T>(string key)
    {
        if (!Parameters.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"The key '{key}' does not exist in the parameter list.");
        }

        return (T)value;
    }

    /// <summary>
    /// Removes the parameter with the specified key.
    /// </summary>
    /// <param name="key">The key of the parameter to remove.</param>
    /// <returns>True if the parameter was successfully removed, otherwise false.</returns>
    public bool Remove(string key)
    {
        return Parameters.Remove(key);
    }

    /// <summary>
    /// Clears all parameters from the list.
    /// </summary>
    public void Clear()
    {
        Parameters.Clear();
    }

    /// <summary>
    /// Determines whether the parameter list contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the parameter list.</param>
    /// <returns>True if the parameter list contains the key, otherwise false.</returns>
    public bool ContainsKey(string key)
    {
        return Parameters.ContainsKey(key);
    }

    /// <summary>
    /// Sets the value of the specified key.
    /// </summary>
    /// <param name="key">The key of the parameter.</param>
    /// <param name="data">The value to set.</param>
    public void SetValue(string key, object data)
    {
        Parameters[key] = data;
    }
}
