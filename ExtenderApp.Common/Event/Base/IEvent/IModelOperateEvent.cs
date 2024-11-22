namespace ExtenderApp.IRole
{
    public interface IModelOperateEvent : IEvent
    {
        void Subscription(Delegate @delegate);
    }
}
