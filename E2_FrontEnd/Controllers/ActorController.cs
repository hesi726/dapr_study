using Dapr.Actors;
using Dapr.Actors.Client;
using E2_FrontEnd.ActorDefine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E2_FrontEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("paid/{orderId}")]
        public async Task<ActionResult> PaidAsync(string orderId)
        {
            var actorId = new ActorId("myid-" + orderId);
            var proxy = ActorProxy.Create<IOrderStatusActor>(actorId, nameof(OrderStatusActor));
            var result = await proxy.Paid(orderId);
            var ptimer = ActorProxy.Create<ITimerActor>(actorId, nameof(OrderStatusActor));
            await ptimer.StartTimeAsyc(actorId.GetId(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return Ok(result);
        }

        [HttpGet("get/{orderId}")]
        public async Task<ActionResult> GetAsync(string orderId)
        {
            var actorId = new ActorId("myid-" + orderId);
            var proxy = ActorProxy.Create<IOrderStatusActor>(actorId, nameof(OrderStatusActor));
            return Ok(await proxy.GetStatus(orderId));
        }
    }
}
