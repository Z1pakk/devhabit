using Habit.Application.Persistence;
using Habit.Domain.Enums;
using Habit.Domain.ValueObjects;
using SharedKernel.CQRS;
using SharedKernel.ResultPattern;
using SharedKernel.User;
using StrictId;

namespace Habit.Application.Features.CreateHabit;

public sealed record CreateHabitCommand(
    string Name,
    string Description,
    HabitType Type,
    Frequency Frequency,
    Target Target,
    DateOnly? EndDate,
    Milestone? Milestone
) : ICommand<Result<CreateHabitCommandResponse>> { }

public sealed record CreateHabitCommandResponse(Guid Id);

public sealed class CreateHabitCommandHandler(
    IHabitDbContext dbContext,
    ICurrentUserService currentUserService
) : ICommandHandler<CreateHabitCommand, Result<CreateHabitCommandResponse>>
{
    public async ValueTask<Result<CreateHabitCommandResponse>> Handle(
        CreateHabitCommand request,
        CancellationToken cancellationToken
    )
    {
        var newHabit = new Domain.Entities.Habit()
        {
            Id = Id.NewId(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Frequency = request.Frequency,
            Target = request.Target,
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = request.EndDate,
            Milestone = request.Milestone,
            UserId = currentUserService.UserId,
        };

        dbContext.Habits.Add(newHabit);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreateHabitCommandResponse>.Success(
            new CreateHabitCommandResponse(newHabit.Id.ToGuid())
        );
    }
}
