using MongoDB.Bson.Serialization.Attributes;

namespace MuniAnnouncements.Api.Models.Catalog;

public sealed class AuthorityConfig
{
    [BsonElement("authorityName")]
    public required string AuthorityName { get; init; }

    /// <summary>מיפוי קוד סטטוס → שם הסטטוס כפי שהרשות הגדירה.</summary>
    [BsonElement("statusMap")]
    public Dictionary<string, string> StatusMap { get; init; } = new();

    /// <summary>קודי סטטוס שדורשים פעולה מצד התושב.</summary>
    [BsonElement("actionRequiredStatuses")]
    public List<string> ActionRequiredStatuses { get; init; } = new();

    /// <summary>קודי סטטוס סופיים – בקשה בסטטוס כזה נחשבת סגורה.</summary>
    [BsonElement("finalStatuses")]
    public List<string> FinalStatuses { get; init; } = new();

    /// <summary>זמן התקן לטיפול בימים, נמדד מתאריך ההגשה.</summary>
    [BsonElement("slaDays")]
    public int SlaDays { get; init; }
}
