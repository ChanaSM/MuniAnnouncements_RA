using MuniAnnouncements.Api.Clients;
using MuniAnnouncements.Api.Models.Api;
using MuniAnnouncements.Api.Repositories;

namespace MuniAnnouncements.Api.Services;

public sealed class MyRequestsService
{
    private readonly IFormsSystemClient _formsClient;
    private readonly IServiceCatalogRepository _catalogRepository;
    private readonly RequestNormalizer _normalizer;
    private readonly TimeProvider _timeProvider;

    public MyRequestsService(
        IFormsSystemClient formsClient,
        IServiceCatalogRepository catalogRepository,
        RequestNormalizer normalizer,
        TimeProvider timeProvider)
    {
        _formsClient = formsClient;
        _catalogRepository = catalogRepository;
        _normalizer = normalizer;
        _timeProvider = timeProvider;
    }

    public async Task<MyRequestsResponse> GetMyRequestsAsync(string idNumber, CancellationToken ct = default)
    {
        var requests = await _formsClient.GetRequestsByApplicantAsync(idNumber, ct);

        var catalog = await _catalogRepository.GetByFormCodesAsync(requests.Select(r => r.FormCode), ct);

        var normalized = _normalizer.Normalize(requests, catalog, _timeProvider.GetUtcNow());

        return new MyRequestsResponse { Requests = normalized };
    }
}
