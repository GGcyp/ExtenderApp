using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Mod;

namespace ExtenderApp.MainView
{
    class ModTab : Button
    {
        static ModTab()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ModTab), 
                new FrameworkPropertyMetadata(typeof(ModTab)));
        }

        private readonly ModDetails _modDetails;
        /// <summary>
        /// 全局统一回调
        /// </summary>
        public static Action<ModDetails> Callback {  get; set; }

        public ModTab(ModDetails modDetails)
        {
            _modDetails = modDetails;
            Title = modDetails.Title;
            Description = modDetails.Description;
            Version = modDetails.Version;
        }

        protected override void OnClick()
        {
            Callback?.Invoke(_modDetails);
        }

        #region 名称

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", 
                typeof(string), 
                typeof(ModTab), 
                new PropertyMetadata());

        #endregion

        #region 简介

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", 
                typeof(string), 
                typeof(ModTab), 
                new PropertyMetadata());

        #endregion

        #region 版本号

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", 
                typeof(string), 
                typeof(ModTab), 
                new PropertyMetadata());

        #endregion
    }
}
