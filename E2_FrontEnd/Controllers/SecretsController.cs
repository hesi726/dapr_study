using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Client;
using E2_FrontEnd.ActorDefine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace E2_FrontEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SecretsController : ControllerBase
    {
        private readonly ILogger<SecretsController> _logger;
        DaprClient _daprClient;
        IConfiguration configuration;

        public SecretsController(ILogger<SecretsController> logger,  IConfiguration configuration,  DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
            this.configuration = configuration;
        }
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            Dictionary<string, string> secrets = await _daprClient.GetSecretAsync("secrets01", "RabbitMQConnectStr");
            return Ok(secrets);
        }
        
        [HttpGet("get01")]
        public async Task<ActionResult> Get01Async()
        {
            var secrets = configuration["RabbitMQConnectStr"];
            return Ok(secrets);
        }
    }
}
