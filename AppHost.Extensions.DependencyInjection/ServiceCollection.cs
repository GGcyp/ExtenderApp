using System.Collections;


namespace AppHost.Extensions.DependencyInjection
{
    internal class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        //private List<ServiceDescriptor> m_ServiceDescriptors = new List<ServiceDescriptor>();
        //public ServiceDescriptor this[int index] { get => m_ServiceDescriptors[index]; set { } }

        //public int Count => m_ServiceDescriptors.Count;

        //public bool IsReadOnly => false;

        //public void Add(ServiceDescriptor item)
        //{
        //    foreach (var serviceDescriptor in m_ServiceDescriptors)
        //    {
        //        if (serviceDescriptor.ServiceType == item.ServiceType)
        //        {
        //            return;
        //        }
        //    }

        //    m_ServiceDescriptors.Add(item);
        //}

        //public void Clear()
        //{
        //    m_ServiceDescriptors.Clear();
        //}

        //public bool Contains(ServiceDescriptor item)
        //{
        //    return m_ServiceDescriptors.Contains(item);
        //}

        //public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        //{
        //    m_ServiceDescriptors.CopyTo(array, arrayIndex);
        //}

        //public IEnumerator<ServiceDescriptor> GetEnumerator()
        //{
        //    return m_ServiceDescriptors.GetEnumerator();
        //}

        //public int IndexOf(ServiceDescriptor item)
        //{
        //    return m_ServiceDescriptors.IndexOf(item);
        //}

        //public void Insert(int index, ServiceDescriptor item)
        //{
        //    m_ServiceDescriptors.Insert(index, item);
        //}

        //public bool Remove(ServiceDescriptor item)
        //{
        //    return m_ServiceDescriptors.Remove(item);
        //}

        //public void RemoveAt(int index)
        //{
        //    m_ServiceDescriptors.RemoveAt(index);
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return m_ServiceDescriptors.GetEnumerator();
        //}
    }
}
