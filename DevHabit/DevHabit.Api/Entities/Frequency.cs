namespace DevHabit.Api.Entities;

public sealed class Frequency
{
    public required FrequencyType Type { get; set; }

    public required int TimesPerPeriod { get; set; }
}
