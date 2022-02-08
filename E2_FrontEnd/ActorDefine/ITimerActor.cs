using Dapr.Actors;

namespace E2_FrontEnd.ActorDefine
{
    public interface ITimerActor : IActor
    {
        Task StartTimeAsyc(string name, string text);
    }
}
