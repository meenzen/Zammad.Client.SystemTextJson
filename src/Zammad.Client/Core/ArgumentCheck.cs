using System;
using System.Collections.Generic;
using System.Linq;

namespace Zammad.Client.Core;

#nullable enable

internal static class ArgumentCheck
{
    internal static void ThrowIfNull(object? value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    internal static void ThrowIfNullOrEmpty<T>(IEnumerable<T> value, string paramName)
    {
        ThrowIfNull(value, paramName);

        if (!value.Any())
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
    }
}
