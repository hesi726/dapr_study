using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E2_FrontEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DaprController : ControllerBase
    {
        private readonly ILogger<DaprController> _logger;

        public DaprController(ILogger<DaprController> logger)
        {
            _logger = logger;
        }

        // 通过HttpClient调用BackEnd
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            // Sidecar使用可插接式名称解析组件来解析服务BackEnd的地址。
            // 在自承载模式下，Dapr 使用 mdn 来查找它。
            // 在 Kubernetes 模式下运行时，Kubernetes DNS 服务将确定地址。
            using var httpClient = DaprClient.CreateInvokeHttpClient();
            var result = await httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("http://backend/WeatherForecast");
            //var resultContent = string.Format("result is {0} {1}", result.StatusCode, await result.Content.ReadAsStringAsync());
            //return Ok(resultContent);
            return Ok(result);
        }

        // 通过DaprClient调用BackEnd
        [HttpGet("get2")]
        public async Task<ActionResult> Get2Async()
        {
            using var daprClient = new DaprClientBuilder().Build();
            var result = await daprClient.InvokeMethodAsync<IEnumerable<WeatherForecast>>(HttpMethod.Get, "backend", "WeatherForecast");
            return Ok(result);
        }

        // 通过DaprClient调用BackEnd
        [HttpGet("get3")]
        public async Task<ActionResult> Get3Async()
        {
            var result = "Abc.123";
            return Ok(Task.FromResult(result));
        }

    }
}
