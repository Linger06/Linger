// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0_OR_GREATER || NET451_OR_GREATER
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif

#if NETSTANDARD2_0 || NETFRAMEWORK

// These Annotations are Part of .NET Standard 2.1 and .NET Core 3.0+. Defining them here allows
// their usage to support developers using this library with nullable-reference-type-warnings enabled.
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}

#endif
