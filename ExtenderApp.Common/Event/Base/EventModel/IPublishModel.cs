using ExtenderApp.Common;
using ExtenderApp.Common.Event;

namespace ExtenderApp.IRole
{
    public interface IPublishModel
    {
        ThreadOption forceOption { set; }
        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate;
    }
}
