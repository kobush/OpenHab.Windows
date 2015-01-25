using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public class SectionSwitchWidgetViewModel : WidgetViewModelBase
    {
        private readonly ObservableCollection<WidgetMappingViewModel> _mappings
            = new ObservableCollection<WidgetMappingViewModel>();

        private ICommand _toggleCommand;

        protected override void OnModelUpdated()
        {
            base.OnModelUpdated();

            _mappings.Clear();

            if (Widget.Mappings != null)
            {
                foreach (var mapping in Widget.Mappings)
                {
                    bool isChecked = false;
                    if (Widget.Item != null && mapping.Command != null)
                        isChecked = string.Equals(Widget.Item.State, mapping.Command, StringComparison.OrdinalIgnoreCase);

                    _mappings.Add(new WidgetMappingViewModel(mapping)
                    {
                        Command = ToggleCommand, 
                        IsChecked = isChecked
                    });
                }
            }
        }

        public IEnumerable<WidgetMappingViewModel> Mappings
        {
            get { return _mappings; }
        }

        private ICommand ToggleCommand
        {
            get
            {
                return (_toggleCommand) ??
                       (_toggleCommand = new DelegateCommand<WidgetMappingViewModel>(TogleCommandExecuted));
            }
        }

        private void TogleCommandExecuted(WidgetMappingViewModel widgetMapping)
        {
            if (widgetMapping == null) return;
            SendItemCommand(widgetMapping.Mapping.Command);
        }
    }

    public class WidgetMappingViewModel : BindableBase
    {
        private ICommand _command;
        private bool _isChecked;

        public WidgetMappingViewModel(WidgetMapping mapping)
        {
            Mapping = mapping;
        }

        public WidgetMapping Mapping { get; private set; }

        public string Label
        {
            get { return Mapping.Label; }
        }

        public ICommand Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
    }
}