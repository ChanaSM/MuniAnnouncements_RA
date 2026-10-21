using MuniAnnouncements.Api.Models.External;

namespace MuniAnnouncements.Api.Clients;

public sealed class FakeFormsSystemClient : IFormsSystemClient
{
    private static readonly DateTime Base = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyList<FormRequestDto> AllRequests = new[]
    {
        new FormRequestDto
        {
            RequestId = "F-88213", FormCode = "ARNONA_DISCOUNT", AuthorityId = "6500", ApplicantId = "012345678",
            StatusCode = "3", SubmittedAt = Base.AddDays(1), LastUpdate = Base.AddDays(19),
        },
        new FormRequestDto
        {
            RequestId = "F-77001", FormCode = "PARKING_PERMIT", AuthorityId = "6500", ApplicantId = "012345678",
            StatusCode = "2", SubmittedAt = Base.AddDays(-40), LastUpdate = Base.AddDays(-30),
        },
        new FormRequestDto
        {
            RequestId = "F-65010", FormCode = "HAZARD_REPORT", AuthorityId = "7400", ApplicantId = "012345678",
            StatusCode = "9", SubmittedAt = Base.AddDays(-60), LastUpdate = Base.AddDays(-50),
        },
        new FormRequestDto
        {
            RequestId = "F-90555", FormCode = "ARNONA_DISCOUNT", AuthorityId = "7400", ApplicantId = "012345678",
            StatusCode = "77", SubmittedAt = Base.AddDays(5), LastUpdate = Base.AddDays(25),
        },
        new FormRequestDto
        {
            RequestId = "F-11111", FormCode = "UNKNOWN_FORM", AuthorityId = "6500", ApplicantId = "012345678",
            StatusCode = "1", SubmittedAt = Base.AddDays(2), LastUpdate = Base.AddDays(2),
        },
        new FormRequestDto
        {
            RequestId = "F-30010", FormCode = "ARNONA_DISCOUNT", AuthorityId = "4000", ApplicantId = "012345678",
            StatusCode = "PAY", SubmittedAt = Base.AddDays(10), LastUpdate = Base.AddDays(28),
        },
        new FormRequestDto
        {
            RequestId = "F-30020", FormCode = "HAZARD_REPORT", AuthorityId = "4000", ApplicantId = "012345678",
            StatusCode = "ASSIGNED", SubmittedAt = Base.AddDays(12), LastUpdate = Base.AddDays(13),
        },
        new FormRequestDto
        {
            RequestId = "F-20001", FormCode = "PARKING_PERMIT", AuthorityId = "7400", ApplicantId = "987654321",
            StatusCode = "1", SubmittedAt = Base.AddDays(3), LastUpdate = Base.AddDays(3),
        },
    };

    public Task<IReadOnlyList<FormRequestDto>> GetRequestsByApplicantAsync(string idNumber, CancellationToken ct = default)
    {
        IReadOnlyList<FormRequestDto> result = AllRequests
            .Where(r => r.ApplicantId == idNumber)
            .ToList();

        return Task.FromResult(result);
    }
}
