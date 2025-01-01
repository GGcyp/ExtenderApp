using System.Collections;
using System.Collections.Specialized;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Service;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews
{
    public class ModViewModle : ExtenderAppViewModel<ModView>
    {
        public ModStore ModStore { get; }
        private readonly MainModel _mainModel;

        public ModViewModle(ModStore mods, MainModel mainModel, IServiceStore serviceStore) : base(serviceStore)
        {
            _mainModel = mainModel;
            ModStore = mods;
            ModStore.CollectionChanged += ModStore_CollectionChanged;
        }

        public override void InjectView(ModView view)
        {
            base.InjectView(view);
            AddModTab(ModStore);
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

        public void OpenMod(ModDetails modDetails)
        {
            _serviceStore.ModService.LoadMod(modDetails);
            _mainModel.CurrentModDetails = modDetails;
            _mainModel.ToRunAction?.Invoke();
        }

        private void AddModTab(IList newItems)
        {
            if (newItems is null) return;

            for (int i = 0; i < newItems.Count; i++)
            {
                if (newItems[i] is not ModDetails modDetails) continue;

                var newItem = new ModTab(modDetails);
                View.modGrid.Children.Add(newItem);
            }
        }

        private void RemoveModTab(IList oldItems)
        {
            if (oldItems is null) return;

            for (int i = 0; i < oldItems.Count; i++)
            {
                if (oldItems[i] is not ModDetails modDetails) continue;

                var modTabToRemove = View.modGrid.Children.OfType<ModTab>().FirstOrDefault(tab => tab.ModDetails == modDetails);
                if (modTabToRemove != null)
                {
                    View.modGrid.Children.Remove(modTabToRemove);
                }
            }
        }
    }
}
