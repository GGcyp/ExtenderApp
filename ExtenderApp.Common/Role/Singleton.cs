
namespace ExtenderApp.Role
{
    public abstract class Singleton<T> : BaseObject where T : Singleton<T>, new()
    {
        private static object m_Lock = new object();

        private static T m_Instance;
        public static T Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    lock (m_Lock)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new T();
                            m_Instance.Ctor();
                            m_Instance.Init();
                        }
                    }
                }
                return m_Instance;
            }
        }
    }
}
