using MainApp.Common;
using MainApp.Common.Event;

namespace MainApp.IRole
{
    public interface IPublishModel
    {
        ThreadOption forceOption { set; }
        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate;
    }
}
