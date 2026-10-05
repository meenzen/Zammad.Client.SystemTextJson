using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class AmountCheckQueryTests
{
    [Test]
    [Arguments(0, 0, 0, 1, "1s")]
    [Arguments(0, 0, 0, 9, "9s")]
    [Arguments(0, 0, 5, 0, "5m")]
    [Arguments(0, 2, 0, 0, "2h")]
    [Arguments(7, 0, 0, 0, "7d")]
    [Arguments(0, 24, 0, 0, "1d")]
    [Arguments(0, 0, 120, 0, "2h")]
    public async Task FormatPeriod_UsesLargestUnit(int days, int hours, int minutes, int seconds, string expected)
    {
        var result = AmountCheckQuery.FormatPeriod(new TimeSpan(days, hours, minutes, seconds));

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, 0, 0, 0)] // zero
    [Arguments(0, 0, 0, 10)] // 10 seconds, Zammad would read 1 second
    [Arguments(0, 0, 30, 0)] // 30 minutes, Zammad would read 3 minutes
    [Arguments(0, 0, 1, 30)] // 90 seconds, not a whole number of minutes
    [Arguments(10, 0, 0, 0)] // 10 days
    [Arguments(0, 0, 0, -1)] // negative
    public async Task FormatPeriod_ThrowsForPeriodsZammadCannotRead(int days, int hours, int minutes, int seconds)
    {
        var period = new TimeSpan(days, hours, minutes, seconds);

        await Assert.That(() => AmountCheckQuery.FormatPeriod(period)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task FormatPeriod_ThrowsForFractionalSeconds()
    {
        await Assert
            .That(() => AmountCheckQuery.FormatPeriod(TimeSpan.FromMilliseconds(1500)))
            .Throws<ArgumentOutOfRangeException>();
    }
}
