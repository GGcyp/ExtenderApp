using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public struct FormatterManagerBuilder
    {
        public IServiceProvider Provider { get; }
        public IClientFormatterManager Manager { get; }

        public bool IsEmpty => Manager is null || Provider is null;

        public FormatterManagerBuilder(IServiceProvider provider, IClientFormatterManager manager)
        {
            Provider = provider;
            Manager = manager;
        }
    }
}
