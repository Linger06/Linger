using System.Reflection;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="object"/> extensions
/// </summary>
public static partial class ObjectExtensions
{
    /// <summary>
    /// Executes a specified action on each property of the current class.
    /// </summary>
    /// <typeparam name="T">The type of the class to perform the action on.</typeparam>
    /// <param name="value">The class to perform the action on.</param>
    /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each property of the current class.</param>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// obj.ForIn((name, val) => Console.WriteLine($"{name}: {val}"));
    /// // Output:
    /// // Name: John
    /// // Age: 30
    /// </code>
    /// </example>
    public static void ForIn<T>(this T? value, Action<string, object?> action) where T : class
    {
        if (value == null)
        {
            return;
        }

        foreach (PropertyInfo item in typeof(T).GetProperties())
        {
            var val = item.GetValue(value, null);
            action(item.Name, val);
        }
    }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> of a specified property name.
    /// </summary>
    /// <param name="obj">The object to get the property info from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The <see cref="PropertyInfo"/> of the specified property.</returns>
    /// <exception cref="ArgumentException">Thrown when the property name does not exist.</exception>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John" };
    /// var propertyInfo = obj.GetPropertyInfo("Name");
    /// Console.WriteLine(propertyInfo.Name); // Output: Name
    /// </code>
    /// </example>
    public static PropertyInfo GetPropertyInfo(this object obj, string propertyName)
    {
        var matchedProperty = obj.GetType().GetProperty(propertyName);
        if (matchedProperty == null)
            throw new ArgumentException(null, nameof(propertyName));

        return matchedProperty;
    }

    /// <summary>
    /// Gets the value of a specified property.
    /// </summary>
    /// <param name="obj">The object to get the property value from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the specified property.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John" };
    /// var value = obj.GetPropertyValue("Name");
    /// Console.WriteLine(value); // Output: John
    /// </code>
    /// </example>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        return obj.GetPropertyInfo(propertyName).GetValue(obj, null);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="string"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="string"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object str = "Hello";
    /// bool isString = str.IsString();
    /// Console.WriteLine(isString); // Output: True
    /// </code>
    /// </example>
    public static bool IsString(this object? value)
    {
        return value?.GetType() == typeof(string);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="short"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="short"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = (short)123;
    /// bool isInt16 = num.IsInt16();
    /// Console.WriteLine(isInt16); // Output: True
    /// </code>
    /// </example>
    public static bool IsInt16(this object? value)
    {
        return value?.GetType() == typeof(short);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="int"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="int"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = 123;
    /// bool isInt = num.IsInt();
    /// Console.WriteLine(isInt); // Output: True
    /// </code>
    /// </example>
    public static bool IsInt(this object? value)
    {
        return value?.GetType() == typeof(int);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="long"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="long"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = 123L;
    /// bool isInt64 = num.IsInt64();
    /// Console.WriteLine(isInt64); // Output: True
    /// </code>
    /// </example>
    public static bool IsInt64(this object? value)
    {
        return value?.GetType() == typeof(long);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="decimal"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="decimal"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = 123.45M;
    /// bool isDecimal = num.IsDecimal();
    /// Console.WriteLine(isDecimal); // Output: True
    /// </code>
    /// </example>
    public static bool IsDecimal(this object? value)
    {
        return value?.GetType() == typeof(decimal);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="float"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = 123.45F;
    /// bool isSingle = num.IsSingle();
    /// Console.WriteLine(isSingle); // Output: True
    /// </code>
    /// </example>
    public static bool IsSingle(this object? value)
    {
        return value?.GetType() == typeof(float);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="float"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = "123.45";
    /// bool isFloat = num.IsFloat();
    /// Console.WriteLine(isFloat); // Output: True
    /// </code>
    /// </example>
    public static bool IsFloat(this object? value)
    {
        return value != null && float.TryParse(value.ToString(), out _);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="double"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="double"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object num = 123.45;
    /// bool isDouble = num.IsDouble();
    /// Console.WriteLine(isDouble); // Output: True
    /// </code>
    /// </example>
    public static bool IsDouble(this object? value)
    {
        return value?.GetType() == typeof(double);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="DateTime"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="DateTime"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object date = DateTime.Now;
    /// bool isDateTime = date.IsDateTime();
    /// Console.WriteLine(isDateTime); // Output: True
    /// </code>
    /// </example>
    public static bool IsDateTime(this object? value)
    {
        return value?.GetType() == typeof(DateTime);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is of an equivalent <see cref="bool"/> type.
    /// </summary>
    /// <param name="value">The <see cref="object"/> to check.</param>
    /// <returns>True if the value is of an equivalent <see cref="bool"/> type; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// object flag = true;
    /// bool isBoolean = flag.IsBoolean();
    /// Console.WriteLine(isBoolean); // Output: True
    /// </code>
    /// </example>
    public static bool IsBoolean(this object? value)
    {
        return value?.GetType() == typeof(bool);
    }

    //public const string DoubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";

    ///// <summary>
    ///// 数字科学计数法处理
    ///// </summary>
    ///// <param name="strData"></param>
    ///// <returns></returns>
    //public static Decimal ChangeToDecimal(this object input)
    //{
    //    var inputStr = input.ToString();
    //    if(inputStr.IsScientificNotation())
    //    {
    //        Decimal dData = 0.0M;
    //        if (inputStr.Contains("E"))
    //        {
    //            dData = Convert.ToDecimal(Decimal.Parse(inputStr, System.Globalization.NumberStyles.Float));

    //            string numberFromToString = input.ToString(DoubleFixedPoint);//0.00009
    //        }
    //        else
    //        {
    //            dData = Convert.ToDecimal(inputStr);
    //        }
    //        return Math.Round(dData, 4);
    //    }

    //}
}