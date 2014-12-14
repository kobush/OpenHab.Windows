using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public abstract class WidgetViewModelBase : BindableBase
    {
        private Widget _widget;
        private string _icon;
        private string _label;
        private Uri _iconUrl;

        [Dependency]
        public IIconUrlProvider IconUrlProvider { get; set; }

        public void Set(Widget widget)
        {
            _widget = widget;

            Label = _widget.Label;
            Icon = _widget.Icon;

            OnModelUpdated();
        }

        protected Widget Widget
        {
            get { return _widget; }
        }

        public string Label
        {
            get { return _label; }
            protected set { SetProperty(ref _label, value); }
        }

        public string Icon
        {
            get { return _icon; }
            protected set
            {
                if (SetProperty(ref _icon, value))
                {
                    if (IconUrlProvider != null)
                        IconUrl = IconUrlProvider.ResolveIconUrl(Icon);
                }

            }
        }

        public Uri IconUrl
        {
            get { return _iconUrl; }
            private set { SetProperty(ref _iconUrl, value); }
        }

        protected abstract void OnModelUpdated();
    }


}