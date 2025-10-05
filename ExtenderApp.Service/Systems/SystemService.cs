using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Services.Systems.KeyCaptures;

namespace ExtenderApp.Services
{
    internal class SystemService : DisposableObject, ISystemService
    {
        public IClipboard Clipboard { get; }

        public IKeyCapture KeyCapture { get; }

        public SystemService(IClipboard clipboard)
        {
            Clipboard = clipboard;
            KeyCapture = new KeyCapture_Win();
        }

        protected override void Dispose(bool disposing)
        {
            KeyCapture.Dispose();
        }
    }
}
