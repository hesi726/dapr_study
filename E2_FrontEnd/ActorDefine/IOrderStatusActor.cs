using Dapr.Actors;

namespace E2_FrontEnd.ActorDefine
{
    public interface IOrderStatusActor : IActor
    {
        Task<string> Paid(string orderId);

        Task<string> GetStatus(string orderId);
    }
}
