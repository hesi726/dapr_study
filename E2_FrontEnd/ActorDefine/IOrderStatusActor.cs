using Dapr.Actors;

namespace E2_FrontEnd.ActorDefine
{
    public interface IOrderStatusActor : IActor
    {
        Task<string> Paid();

        Task<string> GetStatus();
    }
}
