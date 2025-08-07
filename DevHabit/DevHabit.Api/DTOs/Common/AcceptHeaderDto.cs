using DevHabit.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DevHabit.Api.DTOs.Common;

public record AcceptHeaderDto
{
    [FromHeader(Name = "Accept")]
    public string AcceptHeader { get; set; }

    public bool IsLinksIncluded =>
        MediaTypeHeaderValue.TryParse(AcceptHeader, out MediaTypeHeaderValue? mediaType)
        && mediaType.SubTypeWithoutSuffix.HasValue
        && mediaType.SubTypeWithoutSuffix.Value.Contains(
            CustomMediaTypeNames.Application.HateoasJson
        );
}
