using MongoDB.Bson.Serialization.Attributes;

namespace MuniAnnouncements.Api.Models.Catalog;

[BsonIgnoreExtraElements]
public sealed class ServiceCatalogDocument
{
    /// <summary>קוד הטופס – משמש כמזהה המסמך (_id) בקטלוג.</summary>
    [BsonId]
    [BsonElement("formCode")]
    public required string FormCode { get; init; }

    [BsonElement("serviceName")]
    public required string ServiceName { get; init; }

    /// <summary>מפתח: מזהה רשות (authorityId). ערך: ההגדרות של אותה רשות לטופס זה.</summary>
    [BsonElement("authorities")]
    public Dictionary<string, AuthorityConfig> Authorities { get; init; } = new();
}
