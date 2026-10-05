using System.Globalization;

namespace Zammad.Client.Core;

public sealed class AmountCheckQuery
{
    private static readonly (TimeSpan Unit, string Suffix)[] Units =
    [
        (TimeSpan.FromDays(1), "d"),
        (TimeSpan.FromHours(1), "h"),
        (TimeSpan.FromMinutes(1), "m"),
        (TimeSpan.FromSeconds(1), "s"),
    ];

    /// <summary>
    /// The time span to count tickets in.
    /// </summary>
    /// <remarks>
    /// Must be 1 to 9 days, hours, minutes or seconds, e.g. 2 hours or 5 minutes. Zammad only reads the first digit of
    /// the period, so it would count 30 minutes as 3 minutes.
    /// </remarks>
    public required TimeSpan Period { get; init; }

    /// <summary>
    /// The state is <c>warning</c> if more tickets than this were created.
    /// </summary>
    public int? MaxWarning { get; init; }

    /// <summary>
    /// The state is <c>critical</c> if more tickets than this were created.
    /// </summary>
    public int? MaxCritical { get; init; }

    /// <summary>
    /// The state is <c>warning</c> if this many tickets or fewer were created.
    /// </summary>
    public int? MinWarning { get; init; }

    /// <summary>
    /// The state is <c>critical</c> if this many tickets or fewer were created.
    /// </summary>
    public int? MinCritical { get; init; }

    internal static string FormatPeriod(TimeSpan period)
    {
        foreach (var (unit, suffix) in Units)
        {
            if (period.Ticks % unit.Ticks == 0 && period.Ticks / unit.Ticks is >= 1 and <= 9 and var count)
            {
                return count.ToString(CultureInfo.InvariantCulture) + suffix;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(period),
            period,
            "The period must be 1 to 9 days, hours, minutes or seconds."
        );
    }
}
