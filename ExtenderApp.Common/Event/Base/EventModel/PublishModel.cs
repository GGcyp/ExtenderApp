using ExtenderApp.Common.Event;
using ExtenderApp.IRole;

namespace ExtenderApp.Common
{
    internal class PublishModel : IPublishModel
    {
        private ThreadOption m_ForceOption;
        public ThreadOption forceOption
        {
            get => m_ForceOption;
            set => m_ForceOption = value;
        }

        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate
        {
            action = referenceData.target as Action;
            action += () =>
            {
                Clear();
            };
            return (m_ForceOption == ThreadOption.None) ? referenceData.eventThreadOption : m_ForceOption;
        }

        public void Clear()
        {
            m_ForceOption = ThreadOption.None;
        }
    }

    internal class PublishModel<T> : IPublishModel
    {
        private Action<T> m_Action;

        private ThreadOption m_ForceOption;
        public ThreadOption forceOption
        {
            get => m_ForceOption;
            set => m_ForceOption = value;
        }

        private T m_EventData;
        public T eventData
        {
            get => m_EventData;
            set => m_EventData = value;
        }

        public ThreadOption GetPublishAction<T1>(ActionEventReferenceData<T1> referenceData, out Action action) where T1 : Delegate
        {
            m_Action = referenceData.target as Action<T>;
            action = () => 
            { 
                m_Action?.Invoke(m_EventData); 
                Clear();
            };
            return m_ForceOption == ThreadOption.None ? referenceData.eventThreadOption : m_ForceOption;
        }

        public void Clear()
        {
            m_ForceOption = ThreadOption.None;
            m_EventData = default;
        }
    }

    internal class PublishModel<T1, T2> : IPublishModel
    {
        private ThreadOption m_ForceOption;
        public ThreadOption forceOption
        {
            get => m_ForceOption;
            set => m_ForceOption = value;
        }

        private Action<T1, T2> m_Action;

        private T1 m_EventData1;
        public T1 eventData1
        {
            get => m_EventData1;
            set => m_EventData1 = value;
        }

        private T2 m_EventData2;
        public T2 eventData2
        {
            get => m_EventData2;
            set => m_EventData2 = value;
        }

        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate
        {
            ThreadOption threadOption = m_ForceOption == ThreadOption.None ? referenceData.eventThreadOption : m_ForceOption;
            m_Action = referenceData.target as Action<T1,T2>;
            action = () => 
            { 
                m_Action?.Invoke(m_EventData1, m_EventData2);
                Clear();
            };
            return m_ForceOption == ThreadOption.None? referenceData.eventThreadOption : m_ForceOption;
        }

        public void Clear()
        {
            m_ForceOption = ThreadOption.None;
            m_EventData1 = default;
            m_EventData2 = default;
        }
    }

    internal class PublishModel<T1, T2, T3> : IPublishModel
    {
        private ThreadOption m_ForceOption;
        public ThreadOption forceOption
        {
            get => m_ForceOption;
            set => m_ForceOption = value;
        }

        private Action<T1, T2, T3> m_Action;

        private T1 m_EventData1;
        public T1 eventData1
        {
            get => m_EventData1;
            set => m_EventData1 = value;
        }

        private T2 m_EventData2;
        public T2 eventData2
        {
            get => m_EventData2;
            set => m_EventData2 = value;
        }

        private T3 m_EventData3;
        public T3 eventData3
        {
            get => m_EventData3;
            set => m_EventData3 = value;
        }

        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate
        {
            ThreadOption threadOption = m_ForceOption == ThreadOption.None ? referenceData.eventThreadOption : m_ForceOption;
            m_Action = referenceData.target as Action<T1, T2, T3>;
            action = () => 
            { 
                m_Action(m_EventData1, m_EventData2, m_EventData3);
                Clear();
            };
            return m_ForceOption == ThreadOption.None ? referenceData.eventThreadOption : m_ForceOption;
        }

        public void Clear()
        {
            m_ForceOption = ThreadOption.None;
            m_EventData1 = default;
            m_EventData2 = default;
            m_EventData3 = default;
        }
    }

    internal class PublishModel<T1, T2, T3, T4> : IPublishModel
    {
        private ThreadOption m_ForceOption;
        public ThreadOption forceOption
        {
            get => m_ForceOption;
            set => m_ForceOption = value;
        }

        private Action<T1, T2, T3, T4> m_Action;

        private T1 m_EventData1;
        public T1 eventData1
        {
            get => m_EventData1;
            set => m_EventData1 = value;
        }

        private T2 m_EventData2;
        public T2 eventData2
        {
            get => m_EventData2;
            set => m_EventData2 = value;
        }

        private T3 m_EventData3;
        public T3 eventData3
        {
            get => m_EventData3;
            set => m_EventData3 = value;
        }

        private T4 m_EventData4;
        public T4 eventData4
        {
            get => m_EventData4;
            set => m_EventData4 = value;
        }

        public ThreadOption GetPublishAction<T>(ActionEventReferenceData<T> referenceData, out Action action) where T : Delegate
        {          
            m_Action = referenceData.target as Action<T1, T2, T3, T4>;
            action = () => 
            { 
                m_Action(m_EventData1, m_EventData2, m_EventData3, m_EventData4); 
                Clear();
            };
            return m_ForceOption == ThreadOption.None ? referenceData.eventThreadOption : m_ForceOption;
        }

        public void Clear()
        {
            m_ForceOption = ThreadOption.None;
            m_EventData1 = default;
            m_EventData2 = default;
            m_EventData3 = default;
            m_EventData4 = default;
        }
    }
}
