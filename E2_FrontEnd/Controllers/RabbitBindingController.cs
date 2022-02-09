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
    public class RabbitBindingController : ControllerBase
    {
        private readonly ILogger<RabbitBindingController> _logger;
        public RabbitBindingController(ILogger<RabbitBindingController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult> GetAsync([FromServices] DaprClient daprClient)
        {
            await daprClient.InvokeBindingAsync("RabbitBinding", "create", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult> Post()
        {
           Stream stream = Request.Body;
           byte[] buffer = new byte[Request.ContentLength.Value];
           stream.Position = 0L;
           await stream.ReadAsync(buffer, 0, buffer.Length);
           string content = Encoding.UTF8.GetString(buffer);
           _logger.LogInformation(".............binding............." + content);
           return Ok();
        }
    }
}
