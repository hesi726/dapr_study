using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace E2_FrontEnd.Controllers
{
    /// <summary>
    /// 测试声明式订阅
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class TestSubController : ControllerBase
    {
        private readonly ILogger<StateController> _logger;
        private readonly DaprClient _daprClient;
        public TestSubController(ILogger<StateController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

      

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            Stream stream = Request.Body;
            byte[] buffer = new byte[Request.ContentLength.Value];
            stream.Position = 0L;
            stream.ReadAsync(buffer, 0, buffer.Length);
            var content = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("----------￥￥￥￥￥-------------------" + content + "----------------------------");
            return Ok(content);
        }

    }
}
