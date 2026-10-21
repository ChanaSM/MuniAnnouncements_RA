namespace MuniAnnouncements.Api.Models.Api;


public sealed record NormalizedRequestDto
{
    public required string RequestId { get; init; }

    /// <summary>שם השירות מהקטלוג (במקום formCode).</summary>
    public required string ServiceName { get; init; }

    /// <summary>שם הרשות מהקטלוג (במקום authorityId).</summary>
    public required string AuthorityName { get; init; }

    /// <summary>שם הסטטוס כפי שהרשות הגדירה. סטטוס לא מוכר מוצג כ"בטיפול".</summary>
    public required string Status { get; init; }

    /// <summary>true אם הסטטוס דורש פעולה מצד התושב.</summary>
    public required bool ActionRequired { get; init; }

    /// <summary>true אם הבקשה עדיין פתוחה ועבר זמן ה-SLA של הרשות מאז ההגשה.</summary>
    public required bool IsOverdue { get; init; }

    /// <summary>תאריך הגשה בפורמט dd/MM/yyyy.</summary>
    public required string SubmittedAt { get; init; }

    /// <summary>תאריך עדכון אחרון בפורמט dd/MM/yyyy.</summary>
    public required string LastUpdate { get; init; }
}
