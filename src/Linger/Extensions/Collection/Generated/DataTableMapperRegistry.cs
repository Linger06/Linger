using System.Data;
using System.Reflection;

namespace Linger.Extensions.Collection.Generated;

internal static class DataTableMapperRegistry
{
    internal interface IProvider
    {
        Type ModelType { get; }
        IReadOnlyList<ColumnDef> Columns { get; }
        void FillRow(DataRow row, object item);
    }

    internal readonly struct ColumnDef
    {
        public ColumnDef(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }

    private static readonly Dictionary<Type, IProvider> s_providers = new();
    private static readonly object s_lock = new();
    private static readonly HashSet<Assembly> s_scanned = new();

    public static void Register(IProvider provider)
    {
        lock (s_lock)
        {
            s_providers[provider.ModelType] = provider;
        }
    }

    public static bool TryGet(Type type, out IProvider? provider)
    {
        lock (s_lock)
        {
            if (s_providers.TryGetValue(type, out provider))
                return true;

            // Lazy scan the assembly once to discover generated providers.
            var asm = type.Assembly;
            if (!s_scanned.Contains(asm))
            {
                s_scanned.Add(asm);
                TryDiscoverProviders(asm);
            }

            return s_providers.TryGetValue(type, out provider);
        }
    }

    private static void TryDiscoverProviders(Assembly asm)
    {
        try
        {
            foreach (var t in asm.GetTypes())
            {
                if (typeof(IProvider).IsAssignableFrom(t))
                {
                    // Instantiate even for non-public types
                    if (Activator.CreateInstance(t, true) is IProvider p)
                    {
                        s_providers[p.ModelType] = p;
                    }
                }
            }
        }
        catch
        {
            // best-effort; ignore
        }
    }
}
