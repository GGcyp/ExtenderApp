using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Common.File;

namespace ExtenderApp.ViewModels
{
    public abstract class BaseViewModel : IViewModel
    {
        /// <summary>
        /// 服务存储接口实例
        /// </summary>
        protected readonly IServiceStore _serviceStore;

        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected readonly IView _view;

        /// <summary>
        /// 获取指定类型的视图实例
        /// </summary>
        /// <typeparam name="TView">视图类型，需要继承自IView接口</typeparam>
        /// <returns>返回指定类型的视图实例</returns>
        /// <exception cref="ArgumentNullException">如果_view为null，则抛出此异常</exception>
        /// <exception cref="InvalidCastException">如果_view无法转换为指定类型，则抛出此异常</exception>
        protected TView GetView<TView>() where TView : class, IView
        {
            ArgumentNullException.ThrowIfNull(_view, string.Format("视图为空 : {0}", GetType().Name));

            var view = _view as TView;
            if (view is null)
                throw new InvalidCastException(string.Format("类型无效，{0}无法转换为{1}", _view.GetType().Name, typeof(TView)));

            return view;
        }

        /// <summary>
        /// 模型接口实例
        /// </summary>
        protected readonly IModel _model;

        /// <summary>
        /// 获取指定类型的模型实例
        /// </summary>
        /// <typeparam name="TModel">模型类型，需要继承自IModel接口</typeparam>
        /// <returns>返回指定类型的模型实例</returns>
        /// <exception cref="ArgumentNullException">如果_model为null，则抛出此异常</exception>
        /// <exception cref="InvalidCastException">如果_model无法转换为指定类型，则抛出此异常</exception>
        protected TModel GetModel<TModel>() where TModel : class, IModel
        {
            ArgumentNullException.ThrowIfNull(_model, string.Format("模型为空 : {0}", GetType().Name));

            var model = _model as TModel;
            if (model is null)
                throw new InvalidCastException(string.Format("类型无效，{0}无法转换为{1}", _view.GetType().Name, typeof(TModel)));

            return model;
        }


        public BaseViewModel(IServiceStore service) : this(null, null, service)
        {

        }

        public BaseViewModel(IView view, IServiceStore service) : this(view, null, service)
        {

        }

        public BaseViewModel(IModel model, IServiceStore service) : this(null, model, service)
        {

        }

        protected BaseViewModel(IView view, IModel model, IServiceStore serviceStore)
        {
            _serviceStore = serviceStore;
            _view = view;
            _model = model;
        }

        /// <summary>
        /// 导航到指定的视图。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型，必须实现 IView 接口。</typeparam>
        public IView NavigateTo<TView>() where TView : class, IView
        {
            return NavigateTo(typeof(TView));
        }

        /// <summary>
        /// 导航到指定的视图类型。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        public IView NavigateTo(Type targetView)
        {
            return _serviceStore.NavigationService.NavigateTo(targetView, _view);
        }
    }

    public class BaseViewModel<TModel, TView> : BaseViewModel where TModel : IModel where TView : IView
    {
        protected readonly TModel _model;

        public BaseViewModel(TModel model, TView view, IServiceStore service) : base(view, service)
        {
            _model = model;
        }



        //public override void Read(string path, Action? callBack = null, Action<string>? errorCallBack = null)
        //{
        //    try
        //    {
        //        OnReadstart();
        //        _model.ModelRead(path);
        //        OnReadend();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorCallBack?.Invoke(ex.Message);
        //    }
        //    finally
        //    { 
        //        callBack?.Invoke();
        //    }
        //}

        //public override void Read(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null)
        //{
        //    try
        //    {
        //        OnReadstart();
        //        _model.ModelRead(fileName, extensionType, info);
        //        OnReadend();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorCallBack?.Invoke(ex.Message);
        //    }
        //    finally
        //    {
        //        callBack?.Invoke();
        //    }
        //}

        //public override void Write(string path, Action? callBack = null, Action<string>? errorCallBack = null)
        //{
        //    try
        //    {
        //        OnReadstart();
        //        _model.ModelWrite(path);
        //        OnReadend();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorCallBack?.Invoke(ex.Message);
        //    }
        //    finally
        //    {
        //        callBack?.Invoke();
        //    }
        //}

        //public override void Write(string fileName, FileExtensionType extensionType, FileArchitectureInfo info, Action? callBack = null, Action<string>? errorCallBack = null)
        //{
        //    try
        //    {
        //        OnReadstart();
        //        _model.ModelRead(fileName, extensionType, info);
        //        OnReadend();
        //    }
        //    catch (Exception ex)
        //    {
        //        errorCallBack?.Invoke(ex.Message);
        //    }
        //    finally
        //    {
        //        callBack?.Invoke();
        //    }
        //}

        ///// <summary>
        ///// 读取开始前调用
        ///// </summary>
        //protected virtual void OnReadstart()
        //{

        //}

        ///// <summary>
        ///// 读取结束后调用
        ///// </summary>
        //protected virtual void OnReadend()
        //{

        //}

        ///// <summary>
        ///// 写入开始前调用
        ///// </summary>
        //protected virtual void OnWritestart()
        //{

        //}

        ///// <summary>
        ///// 写入结束后调用
        ///// </summary>
        //protected virtual void OnWriteend()
        //{

        //}
    }
}
