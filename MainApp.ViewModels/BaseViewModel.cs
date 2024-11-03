using MainApp.Abstract;
using MainApp.Common;
using MainApp.Common.File;

namespace MainApp.ViewModels
{
    public abstract class BaseViewModel : IViewModel
    {
        protected readonly IDispatcher _dispatcher;

        public BaseViewModel(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public abstract void Read(string path, Action? callBack = null, Action<string>? errorCallBack = null);

        public abstract void Read(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null);

        public abstract void Write(string path, Action? callBack = null, Action<string>? errorCallBack = null);

        public abstract void Write(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null);
    }

    public class BaseViewModel<TModel> : BaseViewModel  where TModel : IModel
    {
        protected readonly TModel _model;

        public BaseViewModel(TModel model, IDispatcher dispatcher) : base(dispatcher)
        {
            _model = model;
        }

        public override void Read(string path, Action? callBack = null, Action<string>? errorCallBack = null)
        {
            try
            {
                OnReadstart();
                _model.ModelRead(path);
                OnReadend();
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex.Message);
            }
            finally
            { 
                callBack?.Invoke();
            }
        }

        public override void Read(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null)
        {
            try
            {
                OnReadstart();
                _model.ModelRead(fileName, extensionType, info);
                OnReadend();
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex.Message);
            }
            finally
            {
                callBack?.Invoke();
            }
        }

        public override void Write(string path, Action? callBack = null, Action<string>? errorCallBack = null)
        {
            try
            {
                OnReadstart();
                _model.ModelWrite(path);
                OnReadend();
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex.Message);
            }
            finally
            {
                callBack?.Invoke();
            }
        }

        public override void Write(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null)
        {
            try
            {
                OnReadstart();
                _model.ModelRead(fileName, extensionType, info);
                OnReadend();
            }
            catch (Exception ex)
            {
                errorCallBack?.Invoke(ex.Message);
            }
            finally
            {
                callBack?.Invoke();
            }
        }

        /// <summary>
        /// 读取开始前调用
        /// </summary>
        protected virtual void OnReadstart()
        {

        }

        /// <summary>
        /// 读取结束后调用
        /// </summary>
        protected virtual void OnReadend()
        {

        }

        /// <summary>
        /// 写入开始前调用
        /// </summary>
        protected virtual void OnWritestart()
        {

        }

        /// <summary>
        /// 写入结束后调用
        /// </summary>
        protected virtual void OnWriteend()
        {

        }
    }
}
