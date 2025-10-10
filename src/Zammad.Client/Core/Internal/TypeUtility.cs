using System;

namespace Zammad.Client.Core.Internal;

#nullable enable

public static class TypeUtility
{
    public static void CopyProperties<T>(T from, T to)
    {
        if (from is null)
        {
            throw new ArgumentNullException(nameof(from));
        }

        if (to is null)
        {
            throw new ArgumentNullException(nameof(to));
        }

        var fromType = from.GetType();
        var toType = to.GetType();

        if (!toType.IsSubclassOf(fromType))
        {
            throw new ArgumentException(
                $"The type of the argument \"{nameof(to)}\" must be derived from the type of the argument \"{nameof(from)}\"."
            );
        }

        foreach (var property in fromType.GetProperties())
        {
            if (!property.CanRead)
            {
                continue;
            }

            var value = property.GetValue(from);
            if (property.CanWrite)
            {
                property.SetValue(to, value);
            }
        }
    }
}
