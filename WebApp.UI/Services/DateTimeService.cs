using NodaTime;
using NodaTime.Text;

public class DateTimeService
{
    private readonly IClock _clock;

    public DateTimeService(IClock clock)
    {
        _clock = clock;
    }

    public string GetCurrentUtcTime()
    {
        Instant now = _clock.GetCurrentInstant();
        ZonedDateTime utcTime = now.InUtc();

        var pattern = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);
        return pattern.Format(utcTime);
    }
}
