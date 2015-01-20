using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace OpenHab.UI.ViewModels
{
    public class RollerShutterWidgetViewModel : WidgetViewModelBase
    {
        private ICommand _upCommand;
        private ICommand _stopCommand;
        private ICommand _downCommand;
        private double _percentValue;

        protected override void OnModelUpdated()
        {
            base.OnModelUpdated();

            if (Widget.Item != null)
            {
                int number;
                if (Int32.TryParse(Widget.Item.State, out number))
                    PercentValue = number;
                else
                    PercentValue = 0;
            }
        }

        public double PercentValue
        {
            get { return _percentValue; }
            set { SetProperty(ref _percentValue, value); }
        }

        public ICommand DownCommand
        {
            get { return (_downCommand) ?? (_downCommand = new DelegateCommand(DownCommandExecute)); }
        }

        public ICommand StopCommand
        {
            get { return (_stopCommand) ?? (_stopCommand = new DelegateCommand(StopCommandExecute)); }
        }

        public ICommand UpCommand
        {
            get { return (_upCommand) ?? (_upCommand = new DelegateCommand(UpCommandExecute)); }
        }

        private void DownCommandExecute()
        {
            SendItemCommand("DOWN");
        }

        private void StopCommandExecute()
        {
            SendItemCommand("STOP");
        }

        private void UpCommandExecute()
        {
            SendItemCommand("UP");
        }
    }
}