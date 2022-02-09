using Dapr.Actors.Runtime;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace E2_FrontEnd.ActorDefine
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderStatusActor : Actor, IOrderStatusActor, IRemindable
    {
        ILogger<OrderStatusActor> _logger;
        
        public OrderStatusActor(ActorHost host, ILogger<OrderStatusActor> _logger) : base(host)
        {            
            Console.WriteLine("----Create OrderStatusActor Instance----" + this.Id.GetId());
             this._logger = _logger;             
        }

        public async Task<string> GetStatus()
        {
            var orderId = this.Id.GetId();
            return await StateManager.GetStateAsync<string>(orderId);
        }

        protected override Task OnActivateAsync()
        {
            Console.WriteLine("---- OnActivateAsync ---- " + this.Id.GetId());
            return base.OnActivateAsync();
        }
        protected override Task OnDeactivateAsync()
        {
            Console.WriteLine("---- OnDeactivateAsync ---- " + this.Id.GetId());
            return base.OnDeactivateAsync();
        }

        ActorTimer timer;
        IActorReminder actorReminder;
        public async Task<string> Paid()
        {        
            var orderId = this.Id.GetId();
            Console.WriteLine("-----------" + orderId + "-----------------");
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            var text =  "每3秒打印一次 订单状态";
            /* timer = await RegisterTimerAsync("od-" + orderId,
                 nameof(TimerCallbackAsync),
                 Encoding.UTF8.GetBytes(text),
                 TimeSpan.FromSeconds(5),
                 TimeSpan.FromMilliseconds(-1));
            Console.WriteLine("-----------" + timer.Name + " ---- " + timer.ActorType + "-----------------" + timer.ActorId);
             */
            this.actorReminder = await this.RegisterReminderAsync("reminder-" + orderId, null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(3));
            Console.WriteLine("-----------" + actorReminder.Name +  "-----------------" + actorReminder.State);
            // throw new Exception("测试提醒器是否会重启后自动创建Actor的实例并自动执行");
            return orderId;
        }

        public async Task TimerCallbackAsync(byte[] state)
        {
            var text = await this.GetStatus();
            _logger.LogInformation($" -------{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --  {this.Id.GetId()} -- Status: {text}  --------------");
            if (text == "paid")
            {
                await UnregisterTimerAsync(this.timer);
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var text = await this.GetStatus();
            _logger.LogInformation($" -------{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --  {this.Id.GetId()} -- Status: {text}  --------------");
            if (text == "paid")
            {
                await UnregisterReminderAsync(this.actorReminder);
            }
        }
    }
}
