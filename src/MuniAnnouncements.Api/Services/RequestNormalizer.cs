using System.Globalization;
using MuniAnnouncements.Api.Models.Api;
using MuniAnnouncements.Api.Models.Catalog;
using MuniAnnouncements.Api.Models.External;

namespace MuniAnnouncements.Api.Services;

public sealed class RequestNormalizer
{
    public const string UnknownStatusName = "בטיפול";
    private const string DateFormat = "dd/MM/yyyy";

    private readonly ILogger<RequestNormalizer> _logger;

    public RequestNormalizer(ILogger<RequestNormalizer> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<NormalizedRequestDto> Normalize(
        IReadOnlyList<FormRequestDto> requests,
        IReadOnlyDictionary<string, ServiceCatalogDocument> catalog,
        DateTimeOffset now)
    {
        var normalized = new List<(NormalizedRequestDto Dto, DateTime LastUpdate)>(requests.Count);

        foreach (var request in requests)
        {
            if (!catalog.TryGetValue(request.FormCode, out var service))
            {
                _logger.LogWarning(
                    "Request {RequestId} skipped: form code {FormCode} not found in service catalog",
                    request.RequestId, request.FormCode);
                continue;
            }

            if (!service.Authorities.TryGetValue(request.AuthorityId, out var authority))
            {
                _logger.LogWarning(
                    "Request {RequestId} skipped: authority {AuthorityId} not defined for form code {FormCode}",
                    request.RequestId, request.AuthorityId, request.FormCode);
                continue;
            }

            if (!authority.StatusMap.TryGetValue(request.StatusCode, out var statusName))
            {
                statusName = UnknownStatusName;
                _logger.LogWarning(
                    "Request {RequestId}: unknown status code {StatusCode} for form {FormCode} / authority {AuthorityId}; shown as '{Fallback}'",
                    request.RequestId, request.StatusCode, request.FormCode, request.AuthorityId, UnknownStatusName);
            }

            var actionRequired = authority.ActionRequiredStatuses.Contains(request.StatusCode);

            var isOpen = !authority.FinalStatuses.Contains(request.StatusCode);
            var isOverdue = isOpen && (now - request.SubmittedAt).TotalDays > authority.SlaDays;

            var dto = new NormalizedRequestDto
            {
                RequestId = request.RequestId,
                ServiceName = service.ServiceName,
                AuthorityName = authority.AuthorityName,
                Status = statusName,
                ActionRequired = actionRequired,
                IsOverdue = isOverdue,
                SubmittedAt = FormatDate(request.SubmittedAt),
                LastUpdate = FormatDate(request.LastUpdate),
            };

            normalized.Add((dto, request.LastUpdate));
        }

        return normalized
            .OrderByDescending(x => x.Dto.ActionRequired)
            .ThenByDescending(x => x.LastUpdate)
            .Select(x => x.Dto)
            .ToList();
    }

    private static string FormatDate(DateTime value) =>
        value.ToString(DateFormat, CultureInfo.InvariantCulture);
}
