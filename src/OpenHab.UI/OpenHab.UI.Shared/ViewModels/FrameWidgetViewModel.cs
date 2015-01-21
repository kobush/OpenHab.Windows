using System.Collections.Generic;
using OpenHab.Client;
using OpenHab.UI.Helpers;

namespace OpenHab.UI.ViewModels
{
    public class FrameWidgetViewModel : WidgetViewModelBase, IHubData
    {
        private readonly IWidgetViewModelFactory _widgetViewModelFactory;

        private IEnumerable<WidgetViewModelBase> _widgets;

        public FrameWidgetViewModel(IWidgetViewModelFactory widgetViewModelFactory)
        {
            _widgetViewModelFactory = widgetViewModelFactory;
        }

        // provides text to display as hub header
        object IHubData.Header { get { return Label ?? ""; } }

        public IEnumerable<WidgetViewModelBase> Widgets
        {
            get { return _widgets; }
            private set { SetProperty(ref _widgets, value); }
        }

        protected override void OnModelUpdated()
        {
            base.OnModelUpdated();

            var widgets = new List<WidgetViewModelBase>();
            foreach (var childWidget in Widget.Widgets)
            {
                var childViewModel = _widgetViewModelFactory.Create(childWidget);

                childViewModel.Update(childWidget);
                widgets.Add(childViewModel);
            }

            Widgets = widgets;
        }
    }
}