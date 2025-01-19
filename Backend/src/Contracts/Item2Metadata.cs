using System;

namespace Boilerplate.Contracts;

public abstract class Item2MetadataBase
{
    public Guid? EventhouseItemId { get; set; }
    
    public string EventhouseDisplayName { get; set; }
    
    public Guid? KqlDatabaseItemId { get; set; }
    
    public string KqlDatabaseDisplayName { get; set; }
    
    public string KqlDatabaseQueryUrl { get; set; }
}

/// <summary>
/// Represents the core metadata for item2 stored within the system's storage.
/// </summary>
public class Item2Metadata : Item2MetadataBase
{
    public static readonly Item2Metadata Default = new Item2Metadata();

    public Item2Metadata Clone()
    {
        return new Item2Metadata
        {
            EventhouseItemId = EventhouseItemId,
            EventhouseDisplayName = EventhouseDisplayName,
            KqlDatabaseItemId = KqlDatabaseItemId,
            KqlDatabaseDisplayName = KqlDatabaseDisplayName,
            KqlDatabaseQueryUrl = KqlDatabaseQueryUrl
        };
    }

    public Item2ClientMetadata ToClientMetadata()
    {
        return new Item2ClientMetadata
        {
            EventhouseItemId = EventhouseItemId,
            EventhouseDisplayName = EventhouseDisplayName,
            KqlDatabaseItemId = KqlDatabaseItemId,
            KqlDatabaseDisplayName = KqlDatabaseDisplayName,
            KqlDatabaseQueryUrl = KqlDatabaseQueryUrl
        };
    }
}

/// <summary>
/// Represents extended metadata for item2, including additional information
/// </summary>
public class Item2ClientMetadata : Item2MetadataBase { }

