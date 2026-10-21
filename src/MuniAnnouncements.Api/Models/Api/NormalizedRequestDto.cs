namespace MuniAnnouncements.Api.Models.Api;


public sealed record NormalizedRequestDto
{
    public required string RequestId { get; init; }

    public required string ServiceName { get; init; }

    public required string AuthorityName { get; init; }

    public required string Status { get; init; }

    public required bool ActionRequired { get; init; }

    public required bool IsOverdue { get; init; }

    public required string SubmittedAt { get; init; }

    public required string LastUpdate { get; init; }
}
