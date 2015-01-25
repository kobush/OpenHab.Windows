using System.Globalization;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace OpenHab.UI.ViewModels
{
    public class SliderWidgetViewModel : WidgetViewModelBase
    {
        private double _percentValue;
        private bool _ignoreValueChange;
        private ICommand _updateValueCommand;

        public double PercentValue
        {
            get { return _percentValue; }
            set { SetProperty(ref _percentValue, value); }
        }

        public ICommand UpdateValueCommand
        {
            get { return (_updateValueCommand) ?? (_updateValueCommand = new DelegateCommand<double?>(PercentValueChanged) ); }
        }

        public bool IgnoreValueChange
        {
            get { return _ignoreValueChange; }
            private set { SetProperty(ref _ignoreValueChange, value); }
        }

        protected override void OnModelUpdated()
        {
            base.OnModelUpdated();

            if (Widget.Item != null)
            {
                IgnoreValueChange = true;
                try
                {
                    PercentValue = ParseNumberValue(Widget.Item.State) ?? 0.0;
                }
                finally
                {
                    IgnoreValueChange = false;
                }
            }
        }

        private void PercentValueChanged(double? value)
        {
            if (IgnoreValueChange) 
                return;

            var newState = PercentValue.ToString("F0", CultureInfo.InvariantCulture);
            if (Widget.Item.State != newState)
                SendItemCommand(newState);
        }
    }
}