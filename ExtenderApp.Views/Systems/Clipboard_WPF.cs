using System.Collections.Specialized;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Windows;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views.Clipboards
{
    /// <summary>
    /// ExtenderApp的剪贴板实现，封装了对System.Windows.Clipboard的调用，并通过IDispatcherService确保在UI线程上执行剪贴板操作。
    /// </summary>
    internal class Clipboard_WPF : IClipboard
    {
        private readonly IDispatcherService _dispatcherService;

        public Clipboard_WPF(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        public void Clear()
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.Clear();
                return;
            }
            _dispatcherService.Invoke(Clipboard.Clear);
        }

        public bool ContainsAudio()
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.ContainsAudio();
            return _dispatcherService.InvokeAsync(Clipboard.ContainsAudio).GetAwaiter().GetResult();
        }

        public bool ContainsData(string format)
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.ContainsData(format);
            return _dispatcherService.InvokeAsync(() => Clipboard.ContainsData(format)).GetAwaiter().GetResult();
        }

        public bool ContainsFileDropList()
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.ContainsFileDropList();
            return _dispatcherService.InvokeAsync(Clipboard.ContainsFileDropList).GetAwaiter().GetResult();
        }

        public bool ContainsText()
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.ContainsText();
            return _dispatcherService.InvokeAsync(Clipboard.ContainsText).GetAwaiter().GetResult();
        }

        public bool ContainsText(Contracts.TextDataFormat format)
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.ContainsText((TextDataFormat)format);
            return _dispatcherService.InvokeAsync(() => Clipboard.ContainsText((TextDataFormat)format)).GetAwaiter().GetResult();
        }

        public string GetText()
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.GetText();
            return _dispatcherService.InvokeAsync(Clipboard.GetText).GetAwaiter().GetResult();
        }

        public string GetText(Contracts.TextDataFormat format)
        {
            if (_dispatcherService.CheckAccess())
                return Clipboard.GetText((TextDataFormat)format);
            return _dispatcherService.InvokeAsync(() => Clipboard.GetText((TextDataFormat)format)).GetAwaiter().GetResult();
        }

        public void SetAudio(byte[] audioBytes)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetAudio(audioBytes);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetAudio(audioBytes));
        }

        public void SetAudio(Stream audioStream)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetAudio(audioStream);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetAudio(audioStream));
        }

        public void SetDataObject(object data)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetDataObject(data);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetDataObject(data));
        }

        public void SetDataObject(object data, bool copy)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetDataObject(data, copy);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetDataObject(data, copy));
        }

        public void SetFileDropList(StringCollection fileDropList)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetFileDropList(fileDropList);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetFileDropList(fileDropList));
        }

        public void SetText(string text)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetText(text);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetText(text));
        }

        public void SetText(string text, Contracts.TextDataFormat format)
        {
            if (_dispatcherService.CheckAccess())
            {
                Clipboard.SetText(text, (TextDataFormat)format);
                return;
            }
            _dispatcherService.Invoke(() => Clipboard.SetText(text, (TextDataFormat)format));
        }
    }
}
