using MainApp.Common;

namespace MainApp.Common.Event
{
    public class CallBackEvent : PubSubEvent<Action<Action<ThreadOption, Action>>>
    {
    }
}
