using Habit.Application.Features.CreateHabit;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Endpoint;

namespace Habit.Api.Endpoints.CreateHabit;

public sealed record CreateHabitResponse(Guid Id);

public sealed class CreateHabit : IEndpoint<CreateHabitCommand, IResult>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/habits",
                async (
                    CreateHabitCommand payload,
                    ISender sender,
                    CancellationToken cancellationToken
                ) => await HandleAsync(payload, sender, cancellationToken)
            )
            .Produces<CreateHabitResponse>(StatusCodes.Status201Created)
            .WithTags(nameof(Habit))
            .WithName("Create Basket");
    }

    public async Task<IResult> HandleAsync(
        CreateHabitCommand request,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var commandResult = await sender.Send(request, cancellationToken);
        if (commandResult.IsFailure)
        {
            return commandResult.ToTypedHttpResult();
        }

        var response = new CreateHabitResponse(commandResult.Value!.Id);

        return TypedResults.Created($"{EndpointConfig.BaseApiPath}/habits/{response.Id}", response);
    }
}
