using ExtenderApp.Common;

namespace ExtenderApp.Common.Event
{
    public class CallBackEvent : PubSubEvent<Action<Action<ThreadOption, Action>>>
    {
    }
}
