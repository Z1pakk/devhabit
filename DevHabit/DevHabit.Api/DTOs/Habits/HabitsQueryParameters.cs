using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitsQueryParameters : AcceptHeaderDto
{
    [FromQuery(Name = "q")]
    public string? SearchQuery { get; set; }
    public HabitStatus? Status { get; set; }
    public HabitType? Type { get; set; }
    public string? Sort { get; set; }
    public string? Fields { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
