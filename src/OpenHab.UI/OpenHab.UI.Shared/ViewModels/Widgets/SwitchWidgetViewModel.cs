using System;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public class SwitchWidgetViewModel : WidgetViewModelBase
    {
        private const string ItemStateOn = "ON";
        private const string ItemStateOff = "OFF";

        private bool _isOn;
        private bool _ignoreStateChange;

        protected override void OnModelUpdated()
        {
            base.OnModelUpdated();

            _ignoreStateChange = true;
            try
            {
                IsOn = (string.Equals(Widget.Item.State, ItemStateOn, StringComparison.Ordinal));
            }
            finally
            {
                _ignoreStateChange = false;
            }
        }

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                if (SetProperty(ref _isOn, value) && !_ignoreStateChange)
                {
                    SendItemCommand(value ? ItemStateOn : ItemStateOff);
                }
            }
        }
    }
}