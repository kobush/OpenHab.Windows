using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using OpenHab.Client;

namespace OpenHab.UI.ViewModels
{
    public class HubPageViewModel : ViewModel
    {
        private readonly IWidgetViewModelFactory _widgetViewModelFactory;

        private DelegateCommand _connectCommand;

        private string _pageTitle;
        private IEnumerable<WidgetViewModelBase> _widgets;

        public HubPageViewModel(IWidgetViewModelFactory widgetViewModelFactory)
        {
            _widgetViewModelFactory = widgetViewModelFactory;
        }

        public ICommand ConnectCommand
        {
            get { return (_connectCommand) ?? (_connectCommand = new DelegateCommand(Connect)); }
        }

        private async void Connect()
        {
            var client = new OpenHabRestClient(new Uri("http://192.168.1.2:8080"));

            var cts = new System.Threading.CancellationTokenSource();

            var firstSitemap = (await client.GetSitemaps(cts.Token)).First();

            var homePage = (await client.GetPage(firstSitemap.Homepage, cts.Token));

            ProcessPage(homePage);
        }

        private void ProcessPage(Page page)
        {
            PageTitle = page.Title;

            var widgets = new List<WidgetViewModelBase>();
            foreach (var widget in page.Widgets)
            {
                var widgetViewModel = _widgetViewModelFactory.Create(widget.Type);
                widgetViewModel.Set(widget);
                widgets.Add(widgetViewModel);
            }

            Widgets = widgets;
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            protected set { SetProperty(ref _pageTitle, value); }
        }

        public IEnumerable<WidgetViewModelBase> Widgets
        {
            get { return _widgets; }
            private set { SetProperty(ref _widgets, value); }
        }
    }
}
