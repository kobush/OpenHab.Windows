using System;
using System.Diagnostics;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using OpenHab.Client;
using OpenHab.UI.Services;

namespace OpenHab.UI.ViewModels
{
    public abstract class WidgetViewModelBase : BindableBase
    {
        private Widget _widget;
        private string _icon;
        private string _label;
        private Uri _iconUrl;

        [Dependency]
        public ISettingsManager SettingsManager { get; set; }

        public void Update(Widget widget)
        {
            Debug.Assert(widget != null);
            Debug.Assert(_widget == null || _widget.WidgetId == widget.WidgetId);
            Debug.Assert(_widget == null || _widget.Type == widget.Type);

            _widget = widget;

            Label = _widget.Label;
            Icon = _widget.Icon;

            OnModelUpdated();
        }

        public string WidgetId
        {
            get { return _widget.WidgetId; }
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
                    if (SettingsManager != null && !string.IsNullOrEmpty(_icon))
                    {
                        var settings = SettingsManager.CurrentSettings;
                        IconUrl = (settings != null) ? settings.ResolveIconUrl(Icon) : null;
                    }
                    else
                    {
                        IconUrl = null;
                    }
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