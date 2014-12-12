using Microsoft.Practices.Prism.Mvvm;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public abstract class WidgetViewModelBase : BindableBase
    {
        private Widget _widget;
        private string _icon;
        private string _label;

        public void Set(Widget widget)
        {
            _widget = widget;
            Icon = _widget.Icon;
            Label = _widget.Label;

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
            protected set { SetProperty(ref _icon, value); }
        }

        protected abstract void OnModelUpdated();
    }
}