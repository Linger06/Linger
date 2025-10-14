using Linger.Extensions.Core;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Linger.EFCore.Comparers;

public class StringCollectionComparer() : ValueComparer<IEnumerable<string>>(
    (list1, list2) => (list1 ?? Array.Empty<string>()).SequenceEqual(list2 ?? Array.Empty<string>()),
    list => list.IsNull() ? 0 : list.GetHashCode());
