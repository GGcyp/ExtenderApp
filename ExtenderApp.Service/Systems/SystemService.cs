using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    internal class SystemService : ISystemService
    {
        public IClipboard Clipboard { get; }

        public SystemService(IClipboard clipboard)
        {
            Clipboard = clipboard;
        }
    }
}
