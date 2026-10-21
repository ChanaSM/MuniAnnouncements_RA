using MuniAnnouncements.Api.Models.External;

namespace MuniAnnouncements.Api.Clients;

public interface IFormsSystemClient
{
    Task<IReadOnlyList<FormRequestDto>> GetRequestsByApplicantAsync(string idNumber, CancellationToken ct = default);
}
