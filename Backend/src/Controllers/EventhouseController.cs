using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Boilerplate.Constants;
using Boilerplate.Contracts;
using Boilerplate.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Boilerplate.Controllers
{
    public class EventhouseController : ControllerBase
    {
        private static readonly IList<string> EventhubFabricScopes = new[] { $"{EnvironmentConstants.FabricBackendResourceId}/{WorkloadScopes.EventhouseReadAll}" };
        
        private readonly ILogger<EventhouseController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpClientService _httpClientService;

        public EventhouseController(
            ILogger<EventhouseController> logger,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationService authenticationService,
            IHttpClientService httpClientService
        )
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
            _httpClientService = httpClientService;
        }

        [HttpGet("eventhouse/{workspaceId}/{eventhouseId}")]
        public async Task<IActionResult> GetEventhouse(Guid workspaceId, Guid eventhouseId)
        {
            try
            {
                _logger.LogInformation("GetEventhouse: get eventhouse '{0}' in workspace '{1}'", eventhouseId, workspaceId);
                
                var authorizationContext = await _authenticationService.AuthenticateDataPlaneCall(_httpContextAccessor.HttpContext, allowedScopes: new string[] {WorkloadScopes.EventhouseReadAll});
                var token = await _authenticationService.GetAccessTokenOnBehalfOf(authorizationContext, EventhubFabricScopes);

                var url = $"{EnvironmentConstants.FabricApiBaseUrl}/v1/workspaces/{workspaceId}/items/{eventhouseId}";

                var response = await _httpClientService.GetAsync(url, token);
                var eventhouse = await response.Content.ReadAsAsync<FabricItem>();
                return Ok(eventhouse);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError($"GetEventhouse: Failed authenticate Eventhouse {eventhouseId} in workspace: {workspaceId}. Error: {ex.Message}");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEventhouse: Failed to retrieve Eventhouse {eventhouseId} in workspace: {workspaceId}. Error: {ex.Message}");
                return BadRequest();
            }
        }
    }
}