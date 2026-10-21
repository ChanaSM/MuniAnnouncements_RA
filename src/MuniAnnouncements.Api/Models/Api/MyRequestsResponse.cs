namespace MuniAnnouncements.Api.Models.Api;

public sealed record MyRequestsResponse
{
    public required IReadOnlyList<NormalizedRequestDto> Requests { get; init; }
}
