using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace E2_FrontEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestPubSubController : ControllerBase
    {
        private readonly ILogger<StateController> _logger;
        private readonly DaprClient _daprClient;
        public TestPubSubController(ILogger<StateController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Topic("pubsub", "test_topic")]
        [HttpPost("sub")]
        public async Task<ActionResult> Post()
        {
            Stream stream = Request.Body;
            byte[] buffer = new byte[Request.ContentLength.Value];
            stream.Position = 0L;
            stream.ReadAsync(buffer, 0, buffer.Length);
            var content = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("-----------------------------" + content + "----------------------------");
            return Ok(content);
        }

        [HttpGet("pub")]
        public async Task<ActionResult> PubAsync()
        {
            var data = new WeatherForecast();
            await _daprClient.PublishEventAsync<WeatherForecast>("pubsub", "test_topic", data);
            return Ok();
        }
    }
}
