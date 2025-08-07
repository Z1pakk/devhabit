namespace DevHabit.Api.Entities;

public sealed class Milestone
{
    public required int Target { get; set; }
    public required int Current { get; set; }
}
