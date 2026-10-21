namespace MuniAnnouncements.Api.Models.External;

public sealed record FormRequestDto
{
    public required string RequestId { get; init; }
    public required string FormCode { get; init; }
    public required string AuthorityId { get; init; }
    public required string ApplicantId { get; init; }
    public required string StatusCode { get; init; }
    public required DateTime SubmittedAt { get; init; }
    public required DateTime LastUpdate { get; init; }
}
