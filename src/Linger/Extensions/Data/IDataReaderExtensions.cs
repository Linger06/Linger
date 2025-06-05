using System.ComponentModel;
using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Extensions.Data;

/// <summary>
/// Provides extension methods for <see cref="IDataReader"/>.
/// </summary>
public static class IDataReaderExtensions
{
    /// <summary>
    /// Converts the <see cref="IDataReader"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="dr">The <see cref="IDataReader"/> to convert.</param>
    /// <returns>A <see cref="DataTable"/> containing the data from the <see cref="IDataReader"/>.</returns>
    /// <example>
    /// <code>
    /// using (IDataReader reader = GetDataReader())
    /// {
    ///     DataTable table = reader.ReaderToDataTable();
    /// }
    /// </code>
    /// </example>
    public static DataTable ReaderToDataTable(this IDataReader dr)
    {
        using (dr)
        {
            var objDataTable = new DataTable("Table");
            var intFieldCount = dr.FieldCount;
            for (var intCounter = 0; intCounter < intFieldCount; ++intCounter)
            {
                _ = objDataTable.Columns.Add(dr.GetName(intCounter).ToLower(), dr.GetFieldType(intCounter));
            }

            objDataTable.BeginLoadData();
            var objValues = new object[intFieldCount];
            while (dr.Read())
            {
                _ = dr.GetValues(objValues);
                _ = objDataTable.LoadDataRow(objValues, true);
            }

            dr.Close();
            objDataTable.EndLoadData();
            return objDataTable;
        }
    }

    /// <summary>
    /// Converts the <see cref="IDataReader"/> to a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to convert to.</typeparam>
    /// <param name="dr">The <see cref="IDataReader"/> to convert.</param>
    /// <returns>A list of objects of type <typeparamref name="T"/>.</returns>
    /// <example>
    /// <code>
    /// using (IDataReader reader = GetDataReader())
    /// {
    ///     List&lt;MyClass&gt; list = reader.ReaderToList&lt;MyClass&gt;();
    /// }
    /// </code>
    /// </example>
    public static List<T> ReaderToList<T>(this IDataReader dr)
    {
        using (dr)
        {
            var field = new List<string>(dr.FieldCount);
            for (var i = 0; i < dr.FieldCount; i++)
            {
                field.Add(dr.GetName(i).ToLower());
            }

            var list = new List<T>();
            while (dr.Read())
            {
                T model = Activator.CreateInstance<T>();
                foreach (PropertyInfo property in model!.GetType()
                             .GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.Contains(property.Name.ToLower()))
                    {
                        if (!dr[property.Name].IsNullOrDbNull())
                        {
                            property.SetValue(model, HackType(dr[property.Name], property.PropertyType), null);
                        }
                    }
                }

                list.Add(model);
            }

            dr.Close();
            return list;
        }
    }

    /// <summary>
    /// Converts the <see cref="IDataReader"/> to a single object.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to.</typeparam>
    /// <param name="dr">The <see cref="IDataReader"/> to convert.</param>
    /// <returns>An object of type <typeparamref name="T"/>.</returns>
    /// <example>
    /// <code>
    /// using (IDataReader reader = GetDataReader())
    /// {
    ///     MyClass obj = reader.ReaderToModel&lt;MyClass&gt;();
    /// }
    /// </code>
    /// </example>
    public static T ReaderToModel<T>(this IDataReader dr)
    {
        using (dr)
        {
            T model = Activator.CreateInstance<T>();
            while (dr.Read())
            {
                foreach (PropertyInfo pi in model!.GetType()
                             .GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!dr[pi.Name].IsNullOrDbNull())
                    {
                        pi.SetValue(model, HackType(dr[pi.Name], pi.PropertyType), null);
                    }
                }
            }

            dr.Close();
            return model;
        }
    }

    /// <summary>
    /// Converts the <see cref="IDataReader"/> to a <see cref="Hashtable"/>.
    /// </summary>
    /// <param name="dr">The <see cref="IDataReader"/> to convert.</param>
    /// <returns>A <see cref="Hashtable"/> containing the data from the <see cref="IDataReader"/>.</returns>
    /// <example>
    /// <code>
    /// using (IDataReader reader = GetDataReader())
    /// {
    ///     Hashtable hashtable = reader.ReaderToHashtable();
    /// }
    /// </code>
    /// </example>
    public static Hashtable ReaderToHashtable(this IDataReader dr)
    {
        using (dr)
        {
            var ht = new Hashtable();
            while (dr.Read())
            {
                for (var i = 0; i < dr.FieldCount; i++)
                {
                    var strfield = dr.GetName(i).ToLower();
                    ht[strfield] = dr[strfield];
                }
            }

            dr.Close();
            return ht;
        }
    }

    /// <summary>
    /// Converts the value to the specified type, handling nullable types.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="conversionType">The type to convert to.</param>
    /// <returns>The converted value.</returns>
    public static object? HackType(object? value, Type conversionType)
    {
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (value.IsNull())
            {
                return null;
            }

            var nullableConverter = new NullableConverter(conversionType);
            conversionType = nullableConverter.UnderlyingType;
        }

        return Convert.ChangeType(value, conversionType, ExtensionMethodSetting.DefaultCulture);
    }
}
