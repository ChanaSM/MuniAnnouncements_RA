using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MuniAnnouncements.Api.Models.Catalog;
using MuniAnnouncements.Api.Models.External;
using MuniAnnouncements.Api.Services;
using Xunit;

namespace MuniAnnouncements.Api.Tests;

public class RequestNormalizerTests
{
    // "עכשיו" קבוע בכל הבדיקות, כדי שהתוצאה לא תלויה בשעון המחשב
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    // ----- 1. סטטוס מוכר: שמות מהקטלוג -----

    [Fact]
    public void Known_status_is_translated_and_names_come_from_catalog()
    {
        var (sut, _) = CreateSut();

        var result = sut.Normalize(new[] { MakeRequest(statusCode: "2") }, MakeCatalog(), Now);

        var item = result.Single();
        item.Status.Should().Be("נדרשים מסמכים");
        item.ServiceName.Should().Be("שירות בדיקה");
        item.AuthorityName.Should().Be("רשות בדיקה");
    }

    // ----- 2. סטטוס לא מוכר: "בטיפול" + לוג -----

    [Fact]
    public void Unknown_status_is_shown_as_in_progress_and_logged()
    {
        var (sut, logger) = CreateSut();

        var result = sut.Normalize(new[] { MakeRequest(statusCode: "77") }, MakeCatalog(), Now);

        result.Single().Status.Should().Be(RequestNormalizer.UnknownStatusName);
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Message.Contains("77"));
    }

    // ----- 3. actionRequired -----

    [Theory]
    [InlineData("2", true)]   // ברשימת הסטטוסים שדורשים פעולה
    [InlineData("1", false)]
    [InlineData("9", false)]
    public void ActionRequired_is_true_only_for_configured_statuses(string statusCode, bool expected)
    {
        var (sut, _) = CreateSut();

        var result = sut.Normalize(new[] { MakeRequest(statusCode: statusCode) }, MakeCatalog(), Now);

        result.Single().ActionRequired.Should().Be(expected);
    }

    // ----- 4. isOverdue: חריגה מ-SLA (30 ימים בקטלוג הבדיקה) -----

    [Theory]
    [InlineData("1", 31, true)]    // פתוחה ועבר ה-SLA
    [InlineData("1", 30, false)]   // בדיוק ביום ה-SLA: עדיין לא חריגה (גדול מ, לא גדול או שווה)
    [InlineData("1", 10, false)]   // פתוחה בתוך ה-SLA
    [InlineData("9", 100, false)]  // סגורה: לעולם לא חריגה, גם אם עבר הרבה זמן
    [InlineData("77", 31, true)]   // סטטוס לא מוכר נחשב פתוח
    public void IsOverdue_only_when_open_and_past_sla(string statusCode, int daysSinceSubmit, bool expected)
    {
        var (sut, _) = CreateSut();
        var submitted = Now.UtcDateTime.AddDays(-daysSinceSubmit);

        var result = sut.Normalize(
            new[] { MakeRequest(statusCode: statusCode, submittedAt: submitted) },
            MakeCatalog(),
            Now);

        result.Single().IsOverdue.Should().Be(expected);
    }

    // ----- 5. טופס שלא בקטלוג -----

    [Fact]
    public void Request_with_form_code_missing_from_catalog_is_skipped_and_logged()
    {
        var (sut, logger) = CreateSut();
        var requests = new[]
        {
            MakeRequest(id: "GOOD"),
            MakeRequest(id: "BAD", formCode: "NOT_IN_CATALOG"),
        };

        var result = sut.Normalize(requests, MakeCatalog(), Now);

        result.Select(r => r.RequestId).Should().Equal("GOOD");
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Message.Contains("NOT_IN_CATALOG"));
    }

    // ----- 6. רשות שלא מוגדרת לטופס -----

    [Fact]
    public void Request_with_authority_missing_from_form_definition_is_skipped_and_logged()
    {
        var (sut, logger) = CreateSut();

        var result = sut.Normalize(new[] { MakeRequest(authorityId: "UNKNOWN_AUTHORITY") }, MakeCatalog(), Now);

        result.Should().BeEmpty();
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning && e.Message.Contains("UNKNOWN_AUTHORITY"));
    }

    // ----- 7. מיון -----

    [Fact]
    public void Results_are_sorted_action_required_first_then_last_update_descending()
    {
        var (sut, _) = CreateSut();
        var requests = new[]
        {
            MakeRequest(id: "C-no-action-sep25", statusCode: "1", lastUpdate: new DateTime(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc)),
            MakeRequest(id: "E-action-aug01", statusCode: "2", lastUpdate: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)),
            MakeRequest(id: "D-no-action-oct05", statusCode: "1", lastUpdate: new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc)),
            MakeRequest(id: "F-action-aug15", statusCode: "2", lastUpdate: new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc)),
        };

        var result = sut.Normalize(requests, MakeCatalog(), Now);

        // C ו-D נבחרו בכוונה: מיון לפי המחרוזת "25/09/2026" מול "05/10/2026" היה נותן סדר הפוך
        result.Select(r => r.RequestId).Should().Equal(
            "F-action-aug15",
            "E-action-aug01",
            "D-no-action-oct05",
            "C-no-action-sep25");
    }

    // ----- 8. פורמט תאריכים -----

    [Fact]
    public void Dates_are_formatted_as_dd_MM_yyyy()
    {
        var (sut, _) = CreateSut();
        var request = MakeRequest(
            submittedAt: new DateTime(2026, 9, 2, 10, 15, 0, DateTimeKind.Utc),
            lastUpdate: new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc));

        var result = sut.Normalize(new[] { request }, MakeCatalog(), Now);

        result.Single().SubmittedAt.Should().Be("02/09/2026");
        result.Single().LastUpdate.Should().Be("20/09/2026");
    }

    // ----- 9. ת"ז לא חוזרת ללקוח -----

    [Fact]
    public void Result_does_not_expose_applicant_id()
    {
        var (sut, _) = CreateSut();

        var result = sut.Normalize(new[] { MakeRequest(applicantId: "012345678") }, MakeCatalog(), Now);

        var json = JsonSerializer.Serialize(result);
        json.Should().NotContain("012345678");
        json.ToLowerInvariant().Should().NotContain("applicant");
    }

    // ===== עזרים =====

    private static (RequestNormalizer Sut, ListLogger<RequestNormalizer> Logger) CreateSut()
    {
        var logger = new ListLogger<RequestNormalizer>();
        return (new RequestNormalizer(logger), logger);
    }

    /// <summary>בקשה עם ערכי ברירת מחדל תקינים; כל בדיקה משנה רק את מה שמעניין אותה.</summary>
    private static FormRequestDto MakeRequest(
        string id = "R1",
        string formCode = "FORM",
        string authorityId = "A1",
        string applicantId = "012345678",
        string statusCode = "1",
        DateTime? submittedAt = null,
        DateTime? lastUpdate = null) => new()
    {
        RequestId = id,
        FormCode = formCode,
        AuthorityId = authorityId,
        ApplicantId = applicantId,
        StatusCode = statusCode,
        SubmittedAt = submittedAt ?? Now.UtcDateTime.AddDays(-5),
        LastUpdate = lastUpdate ?? Now.UtcDateTime.AddDays(-1),
    };

    /// <summary>קטלוג קטן: טופס אחד, רשות אחת, SLA של 30 ימים, סטטוס 2 דורש פעולה, סטטוס 9 סופי.</summary>
    private static IReadOnlyDictionary<string, ServiceCatalogDocument> MakeCatalog() =>
        new Dictionary<string, ServiceCatalogDocument>
        {
            ["FORM"] = new ServiceCatalogDocument
            {
                FormCode = "FORM",
                ServiceName = "שירות בדיקה",
                Authorities = new()
                {
                    ["A1"] = new AuthorityConfig
                    {
                        AuthorityName = "רשות בדיקה",
                        StatusMap = new()
                        {
                            ["1"] = "התקבלה",
                            ["2"] = "נדרשים מסמכים",
                            ["9"] = "אושרה",
                        },
                        ActionRequiredStatuses = ["2"],
                        FinalStatuses = ["9"],
                        SlaDays = 30,
                    },
                },
            },
        };

    /// <summary>לוגר שאוסף את ההודעות לרשימה, כדי שנוכל לוודא מה נכתב ללוג.</summary>
    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
