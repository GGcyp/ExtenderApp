using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    internal class SystemService : DisposableObject, ISystemService
    {
        public IClipboard Clipboard { get; }

        public IKeyCapture KeyCapture { get; }

        public SystemService(IClipboard clipboard)
        {
            Clipboard = clipboard;
            KeyCapture = default!;
        }
    }
}