namespace Boilerplate.Contracts;

public class GetKqlDatabaseTablesRequest
{
    /// <summary>
    /// The Query Service Uri, aka cluster url
    /// </summary>
    public string QueryServiceUri { get; set; }
    
    /// <summary>
    /// The name of the kql database
    /// </summary>
    public string DatabaseName { get; set; }
}