using Identity.Application.Features.RegisterUser;
using Identity.Application.Models;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Endpoint;

namespace Identity.Api.Endpoints.RegisterUser;

public sealed record RegisterUserResponse(AccessTokenModel accessTokens);

public sealed class RegisterUser : IEndpoint<RegisterUserCommand, IResult>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/register",
                async (
                    RegisterUserCommand payload,
                    ISender sender,
                    CancellationToken cancellationToken
                ) => await HandleAsync(payload, sender, cancellationToken)
            )
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .WithTags(nameof(Identity))
            .WithName("Register user");
    }

    public async Task<IResult> HandleAsync(
        RegisterUserCommand request,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var commandResult = await sender.Send(request, cancellationToken);
        if (commandResult.IsFailure)
        {
            return commandResult.ToTypedHttpResult();
        }

        var response = new RegisterUserResponse(commandResult.Value!.AccessToken);
        return TypedResults.Ok(response);
    }
}
