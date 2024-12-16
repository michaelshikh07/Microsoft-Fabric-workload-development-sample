namespace Boilerplate.Contracts;

/// <summary>
/// Represents the core metadata for item2 stored within the system's storage.
/// </summary>
public class Item2Metadata
{
    public static readonly Item2Metadata Default = new Item2Metadata();

    public Item2Metadata Clone()
    {
        return new Item2Metadata();
    }
    public Item2ClientMetadata ToClientMetadata()
    {
        return new Item2ClientMetadata();
    }
}

/// <summary>
/// Represents extended metadata for item2, including additional information
/// </summary>
public class Item2ClientMetadata
{
    
}

