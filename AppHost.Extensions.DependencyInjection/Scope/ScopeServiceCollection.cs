using System.Collections;

namespace AppHost.Extensions.DependencyInjection
{
    internal class ScopeServiceCollection : IScopeServiceCollection
    {
        public List<ScopeDescriptor> ScopeServices { get; private set; }
        private readonly ServiceCollection _services = new();
        public ServiceDescriptor this[int index]
        {
            get => _services[index];
            set { }
        }

        public int Count => _services.Count;

        public bool IsReadOnly => false;

        public void Add(ServiceDescriptor item)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (_services[i].ServiceType == item.ServiceType)
                {
                    return;
                }
            }
            _services.Add(item);

            if (item.Lifetime != ServiceLifetime.Scoped) return;
            if (ScopeServices is null) ScopeServices = new();
            ScopeServices.Add(new ScopeDescriptor(item));
        }

        public void Clear()
        {
            _services.Clear();
            ScopeServices?.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            for (int i = 0; i < ScopeServices.Count; i++)
            {
                if (_services[i].ServiceType.Equals(item.ServiceType))
                    return true;
            }
            return false;
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _services.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return _services.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _services.Insert(index, item);
        }

        public bool Remove(ServiceDescriptor item)
        {
            if (!_services.Remove(item))
                return false;

            if (item.Lifetime != ServiceLifetime.Scoped) return true;
            if (ScopeServices is null) return true;
            for (int i = 0; i < ScopeServices.Count; i++)
            {
                if (item.ServiceType == ScopeServices[i].ScopeService.ServiceType)
                {
                    ScopeServices.RemoveAt(i);
                }
            }
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _services[index];
            _services.RemoveAt(index);
            if (item.Lifetime != ServiceLifetime.Scoped) return;
            if (ScopeServices is null) return;

            for (int i = 0; i < ScopeServices.Count; i++)
            {
                if (item.ServiceType == ScopeServices[i].ScopeService.ServiceType)
                {
                    ScopeServices.RemoveAt(i);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _services.GetEnumerator();
        }
    }
}
