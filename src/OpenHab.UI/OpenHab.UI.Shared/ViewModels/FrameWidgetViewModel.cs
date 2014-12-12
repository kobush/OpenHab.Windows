using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenHab.UI.ViewModels
{
    public class FrameWidgetViewModel : WidgetViewModelBase
    {
        private readonly IWidgetViewModelFactory _widgetViewModelFactory;
        
        private ObservableCollection<WidgetViewModelBase> _widgets = new ObservableCollection<WidgetViewModelBase>();

        public FrameWidgetViewModel(IWidgetViewModelFactory widgetViewModelFactory)
        {
            _widgetViewModelFactory = widgetViewModelFactory;
        }

        public IEnumerable<WidgetViewModelBase> Widgets
        {
            get { return _widgets; }
        }

        protected override void OnModelUpdated()
        {
            _widgets.Clear();
            foreach (var childWidget in Widget.Widgets)
            {
                var childViewModel = _widgetViewModelFactory.Create(childWidget.Type);
                childViewModel.Set(childWidget);
                _widgets.Add(childViewModel);
            }
        }
    }
}