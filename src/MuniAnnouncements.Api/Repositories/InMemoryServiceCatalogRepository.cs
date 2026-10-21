using MuniAnnouncements.Api.Models.Catalog;

namespace MuniAnnouncements.Api.Repositories;

public sealed class InMemoryServiceCatalogRepository : IServiceCatalogRepository
{
    private static readonly IReadOnlyDictionary<string, ServiceCatalogDocument> Catalog =
        new[]
        {
            new ServiceCatalogDocument
            {
                FormCode = "ARNONA_DISCOUNT",
                ServiceName = "הנחה בארנונה",
                Authorities = new()
                {
                    ["6500"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית חדרה",
                        StatusMap = new()
                        {
                            ["1"] = "התקבלה",
                            ["2"] = "בטיפול",
                            ["3"] = "נדרשים מסמכים",
                            ["4"] = "נדחתה",
                            ["5"] = "אושרה",
                        },
                        ActionRequiredStatuses = ["3"],
                        FinalStatuses = ["4", "5"],
                        SlaDays = 30,
                    },
                    ["7400"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית נתניה",
                        StatusMap = new()
                        {
                            ["10"] = "נפתחה",
                            ["20"] = "בבדיקה",
                            ["30"] = "ממתין להשלמת פרטים",
                            ["90"] = "הסתיימה",
                        },
                        ActionRequiredStatuses = ["30"],
                        FinalStatuses = ["90"],
                        SlaDays = 45,
                    },
                    ["4000"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית חיפה",
                        StatusMap = new()
                        {
                            ["NEW"] = "חדשה",
                            ["REVIEW"] = "בבדיקת ועדה",
                            ["DOCS"] = "חסרים מסמכים",
                            ["PAY"] = "ממתין לתשלום",
                            ["DONE"] = "אושרה",
                            ["REJECTED"] = "נדחתה",
                        },
                        ActionRequiredStatuses = ["DOCS", "PAY"],
                        FinalStatuses = ["DONE", "REJECTED"],
                        SlaDays = 60,
                    },
                },
            },
            new ServiceCatalogDocument
            {
                FormCode = "PARKING_PERMIT",
                ServiceName = "תו חניה",
                Authorities = new()
                {
                    ["6500"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית חדרה",
                        StatusMap = new()
                        {
                            ["1"] = "התקבלה",
                            ["2"] = "בטיפול",
                            ["3"] = "נדרש תשלום",
                            ["5"] = "הונפק",
                            ["6"] = "נדחתה",
                        },
                        ActionRequiredStatuses = ["3"],
                        FinalStatuses = ["5", "6"],
                        SlaDays = 14,
                    },
                    ["7400"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית נתניה",
                        StatusMap = new()
                        {
                            ["1"] = "נקלטה",
                            ["2"] = "באישור",
                            ["3"] = "אושרה",
                            ["4"] = "נדחתה",
                        },
                        ActionRequiredStatuses = [],
                        FinalStatuses = ["3", "4"],
                        SlaDays = 21,
                    },
                    ["4000"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית חיפה",
                        StatusMap = new()
                        {
                            ["100"] = "בקשה נקלטה",
                            ["200"] = "נדרש אישור בעלות רכב",
                            ["300"] = "תו מוכן לאיסוף",
                            ["400"] = "נמסר",
                            ["500"] = "נדחתה",
                        },
                        ActionRequiredStatuses = ["200", "300"],
                        FinalStatuses = ["400", "500"],
                        SlaDays = 10,
                    },
                },
            },
            new ServiceCatalogDocument
            {
                FormCode = "HAZARD_REPORT",
                ServiceName = "דיווח על מפגע",
                Authorities = new()
                {
                    ["7400"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית נתניה",
                        StatusMap = new()
                        {
                            ["1"] = "התקבל",
                            ["2"] = "הועבר לטיפול",
                            ["3"] = "נדרש אימות מיקום",
                            ["9"] = "טופל",
                        },
                        ActionRequiredStatuses = ["3"],
                        FinalStatuses = ["9"],
                        SlaDays = 7,
                    },
                    ["4000"] = new AuthorityConfig
                    {
                        AuthorityName = "עיריית חיפה",
                        StatusMap = new()
                        {
                            ["OPEN"] = "פתוח",
                            ["ASSIGNED"] = "שויך לצוות",
                            ["CLOSED"] = "סגור",
                        },
                        ActionRequiredStatuses = [],
                        FinalStatuses = ["CLOSED"],
                        SlaDays = 3,
                    },
                },
            },
        }.ToDictionary(d => d.FormCode);

    public Task<IReadOnlyDictionary<string, ServiceCatalogDocument>> GetByFormCodesAsync(
        IEnumerable<string> formCodes,
        CancellationToken ct = default)
    {
        IReadOnlyDictionary<string, ServiceCatalogDocument> result = formCodes
            .Distinct()
            .Where(Catalog.ContainsKey)
            .ToDictionary(code => code, code => Catalog[code]);

        return Task.FromResult(result);
    }
}
