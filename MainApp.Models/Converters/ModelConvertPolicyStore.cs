using MainApp.Abstract;
using MainApp.Common;
using MainApp.Common.Data;

namespace MainApp.Models.Converters
{
    internal class ModelConvertPolicyStore<TModel> : Store<IModelConvertPolicy>,IModelConvertPolicyStore<TModel>
    {
        private ValueList<IModelConvertPolicy> policies;

        public ModelConvertPolicyStore()
        {
            policies = new ValueList<IModelConvertPolicy>();
        }

        public Type ConvertModelType => typeof(TModel);

        public IModelConvertPolicy? Find(FileExtensionType extensionType)
        {
            return policies.Find(p => p.ExtensionType == extensionType);
        }
    }
}
