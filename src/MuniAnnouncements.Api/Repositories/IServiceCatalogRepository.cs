using MuniAnnouncements.Api.Models.Catalog;

namespace MuniAnnouncements.Api.Repositories;

public interface IServiceCatalogRepository
{
    Task<IReadOnlyDictionary<string, ServiceCatalogDocument>> GetByFormCodesAsync(
        IEnumerable<string> formCodes,
        CancellationToken ct = default);
}
