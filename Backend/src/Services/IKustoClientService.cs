using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Kusto.Data.Common;

namespace Boilerplate.Services;

public interface IKustoClientService
{
    Task<IDataReader> ExecuteQueryAsync(string queryUrl, string databaseName, string query, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);

    Task<IDataReader> ExecuteControlCommandAsync(string queryUrl, string databaseName, string command, ClientRequestProperties clientRequestProperties, CancellationToken cancellationToken);
}