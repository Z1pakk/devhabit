using DevHabit.Api.DTOs.Common;
using Newtonsoft.Json;

namespace DevHabit.Api.DTOs.Tags;

public class TagsCollectionDto : ICollectionResponse<TagDto>, ILinksResponse
{
    public IEnumerable<TagDto> Items { get; init; }

    public IEnumerable<LinkDto> Links { get; set; }
}
