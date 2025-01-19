using System;
using System.Data;
using System.Threading.Tasks;
using Boilerplate.Constants;
using Boilerplate.Contracts;
using Boilerplate.Services;
using Fabric_Extension_BE_Boilerplate.Constants;
using Kusto.Cloud.Platform.Data;
using Kusto.Data;
using Kusto.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabric_Extension_BE_Boilerplate.Controllers
{
    public class KqlDatabaseController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger<KqlDatabaseController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly IKustoClientService _kustoClientService;
        
        public KqlDatabaseController(Microsoft.Extensions.Logging.ILogger<KqlDatabaseController> logger,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationService authenticationService,
            IKustoClientService kustoClientService)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
            _kustoClientService = kustoClientService;
        }

        [HttpPost("KqlDatabases/tables")]
        public async Task<IActionResult> GetKqlDatabaseTables([FromBody] GetKqlDatabaseTablesRequest request)
        {
            try
            {
                var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(_httpContextAccessor.HttpContext, allowedScopes: new string[] {WorkloadScopes.EventhouseReadAll});
                var scopes = new[] {$"{request.QueryServiceUri}/.default"};
                var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);

                var clientRequestProperties = GenerateClientRequestProperties(token);

                var kqlShowTablesCommand = CslCommandGenerator.GenerateTablesShowCommand();

                var dataReader = await _kustoClientService.ExecuteControlCommandAsync(request.QueryServiceUri, request.DatabaseName, kqlShowTablesCommand, clientRequestProperties, default).ConfigureAwait(false);
                var data = dataReader.ToEnumerable<TablesShowCommandResult>();

                return Ok(data);
            }
            catch (Exception e)
            {
                _logger.LogError($"GetKqlDatabaseTables: Failed getting tables");
                return Problem();
            }
        }

        [HttpPost("KqlDatabases/query")]
        public async Task<IActionResult> QueryKqlDatabase([FromBody] QueryKqlDatabaseRequest request)
        {
            try
            {
                var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(_httpContextAccessor.HttpContext, allowedScopes: new string[] {WorkloadScopes.KQLDatabaseReadAll});
                var scopes = new[] {$"{request.QueryServiceUri}/.default"};
                var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, scopes);

                var clientRequestProperties = GenerateClientRequestProperties(token);
                var dataReader = await _kustoClientService.ExecuteQueryAsync(request.QueryServiceUri, request.DatabaseName, request.Query, clientRequestProperties, default).ConfigureAwait(false);

                var dataTable = new DataTable();
                dataTable.Load(dataReader);

                var serializedDataTable = JsonConvert.SerializeObject(dataTable);
                var jsonTable = JArray.Parse(serializedDataTable);

                return Ok(jsonTable);
                
                // An alternative method will be to return the result as a stream, it will be more efficient in terms of performance,
                // but will require additional processing on the client side. 
                //var stream = KustoJsonDataStream.GetReaderDataAsStream(dataReader);
                
                //return Ok(stream);
            }
            catch (Exception)
            {
                _logger.LogError("QueryKqlDatabase: Failed getting tables");
                return Problem();
            }
        }

        private ClientRequestProperties GenerateClientRequestProperties(string token)
        {
            return new ClientRequestProperties
            {
                ClientRequestId = GetRequestIdHeader() ?? Guid.NewGuid().ToString(),
                AuthorizationScheme = "Bearer",
                SecurityToken = token,
            };
        }

        private string GetRequestIdHeader()
        {
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HttpHeaders.RequestId, out var headerValue))
            {
                return headerValue;
            }

            return null;
        }
    }
}