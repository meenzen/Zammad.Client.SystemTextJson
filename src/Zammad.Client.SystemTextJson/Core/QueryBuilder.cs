using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Zammad.Client.Core;

internal sealed class QueryBuilder
{
    private readonly SortedDictionary<string, string> _values = [];

    internal QueryBuilder Add(string key, string value)
    {
        _values[key] = value;
        return this;
    }

    internal QueryBuilder Add(string key, int value)
    {
        _values[key] = value.ToString(CultureInfo.InvariantCulture);
        return this;
    }

    internal QueryBuilder Add(string key, bool value)
    {
        _values[key] = value switch
        {
            true => "true",
            false => "false",
        };
        return this;
    }

    internal IReadOnlyDictionary<string, string> Values => _values;

    public override string ToString()
    {
        if (_values.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.Append('?');

        foreach (var pair in _values)
        {
            builder.Append(System.Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(System.Uri.EscapeDataString(pair.Value));
            builder.Append('&');
        }

        // remove last '&'
        builder.Remove(builder.Length - 1, 1);

        return builder.ToString();
    }
}
