using System.Collections;
using System.Collections.Specialized;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Service;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews
{
    public class PluginViewModle : ExtenderAppViewModel<PluginView>
    {
        public PluginStore PluginStore { get; }
        private readonly MainModel _mainModel;

        public PluginViewModle(PluginStore mods, MainModel mainModel, IServiceStore serviceStore) : base(serviceStore)
        {
            _mainModel = mainModel;
            PluginStore = mods;
            PluginStore.CollectionChanged += ModStore_CollectionChanged;
        }

        public override void InjectView(PluginView view)
        {
            base.InjectView(view);
            AddModTab(PluginStore);
        }

        private void ModStore_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddModTab(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveModTab(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // 集合中某个元素被替换了，执行相应逻辑
                    break;
                case NotifyCollectionChangedAction.Move:
                    // 集合中元素位置发生了移动，执行相应逻辑
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // 整个集合被重置了，执行相应逻辑
                    break;
                default:
                    break;
            }
        }

        public void OpenMod(PluginDetails modDetails)
        {
            _serviceStore.ModService.LoadPlugin(modDetails);
            _mainModel.CurrentModDetails = modDetails;
            _mainModel.ToRunAction?.Invoke();
        }

        private void AddModTab(IList newItems)
        {
            if (newItems is null) return;

            for (int i = 0; i < newItems.Count; i++)
            {
                if (newItems[i] is not PluginDetails modDetails) continue;

                var newItem = new PluginTab(modDetails);
                View.modGrid.Children.Add(newItem);
            }
        }

        private void RemoveModTab(IList oldItems)
        {
            if (oldItems is null) return;

            for (int i = 0; i < oldItems.Count; i++)
            {
                if (oldItems[i] is not PluginDetails modDetails) continue;

                var modTabToRemove = View.modGrid.Children.OfType<PluginTab>().FirstOrDefault(tab => tab.ModDetails == modDetails);
                if (modTabToRemove != null)
                {
                    View.modGrid.Children.Remove(modTabToRemove);
                }
            }
        }
    }
}
