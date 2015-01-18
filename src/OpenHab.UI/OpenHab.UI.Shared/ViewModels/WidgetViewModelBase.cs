using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
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
        private bool _isLinked;
        private string _value;

        private DelegateCommand _navigateCommand;

        [Dependency]
        public ISettingsManager SettingsManager { get; set; }

        [Dependency]
        public INavigationService NavigationService { get; set; }

        public void Update(Widget widget)
        {
            Debug.Assert(widget != null);
            Debug.Assert(_widget == null || _widget.WidgetId == widget.WidgetId);
            Debug.Assert(_widget == null || _widget.Type == widget.Type);

            _widget = widget;

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

        public string Value
        {
            get { return _value; }
            protected set { SetProperty(ref _value, value); }
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

        public bool IsLinked
        {
            get { return _isLinked; }
            private set { SetProperty(ref _isLinked, value); }
        }

        public ICommand NavigateCommand
        {
            get { return (_navigateCommand) ?? (_navigateCommand = new DelegateCommand(NavigateExecute)); }
        }

        static readonly Regex LabelRegex = new Regex(".*(?<value>\\[.*\\]).*", 
            RegexOptions.Singleline | RegexOptions.CultureInvariant);

        protected virtual void OnModelUpdated()
        {
            var m = LabelRegex.Match(_widget.Label);
            if (m.Success)
            {
                Group g = m.Groups["value"];
                if (g.Success)
                {
                    Value = g.Value.Substring(1, g.Value.Length - 2);
                    Label = (_widget.Label.Substring(0, g.Index) +
                            _widget.Label.Substring(g.Index + g.Length)).Trim();
                }
            }
            else
            {
                Label = _widget.Label;
            }

            Icon = _widget.Icon;
            IsLinked = _widget.LinkedPage != null;
        }

        private void NavigateExecute()
        {
            if (_widget.LinkedPage != null)
                NavigationService.Navigate(PageToken.Hub, new HubPageParameters {Page = _widget.LinkedPage});
        }
    }


}