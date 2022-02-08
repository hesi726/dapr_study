using Dapr.Actors.Runtime;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace E2_FrontEnd.ActorDefine
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderStatusActor : Actor, IOrderStatusActor, ITimerActor
    {
        ILogger<OrderStatusActor> _logger;
        public OrderStatusActor(ActorHost host, ILogger<OrderStatusActor> _logger) : base(host)
        {
             this._logger = _logger;
        }

        public async Task<string> GetStatus(string orderId)
        {
            return await StateManager.GetStateAsync<string>(orderId);
        }

        public async Task<string> Paid(string orderId)
        {
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            return orderId;
        }

        public Task StartTimeAsyc(string name, string text)
        {
            return RegisterTimerAsync(null,
                 nameof(TimerCallbackAsync),
                 Encoding.UTF8.GetBytes(text),
                 TimeSpan.Zero,
                 TimeSpan.FromSeconds(3));
        }

        public Task TimerCallbackAsync(byte[] state)
        {
            var text = Encoding.UTF8.GetString(state);
            _logger.LogInformation($" ----------- Timer Fired {text} --------------");
            return Task.CompletedTask;
        }
    }
}
