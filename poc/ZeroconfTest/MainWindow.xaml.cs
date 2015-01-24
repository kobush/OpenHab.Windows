using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tmds.MDns;
using Zeroconf;

namespace ZeroconfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _discoverCancellationTokenSource;
        private CancellationTokenSource _browseCancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        const string httpProtocol = "_openhab-server._tcp.local";
        const string httpsProtocol = //"_openhab-server-ssl._tcp.local";
                                   "_openhab-server-ssl._tcp.local";

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            _discoverCancellationTokenSource = new CancellationTokenSource();
            IsSearching = true;


            // find services
            Task.Run(() => ZeroconfResolver.ResolveAsync(httpProtocol,
                    scanTime: TimeSpan.FromSeconds(3), callback: DiscoverCallback,
                    cancellationToken: _discoverCancellationTokenSource.Token))
                .ContinueWith(t =>
                {
                    IsSearching = false;

                    if (t.IsCanceled)
                        return;

                    if (t.IsFaulted)
                        return;

                    if (t.Result.Count == 0)
                    {
                        
                    }
                    else
                    {
                        
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DiscoverCallback(IZeroconfHost host)
        {
            Debug.WriteLine("{0} [{1}]: {2}", host.DisplayName, host.Id, host.IPAddress);
        }

        public bool IsSearching { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _browseCancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => ZeroconfResolver.BrowseDomainsAsync())
                .ContinueWith(t =>
                {
                    IsSearching = false;

                    if (t.IsCanceled)
                        return;

                    if (t.IsFaulted)
                        return;

                    if (t.Result.Count == 0)
                    {
                        
                    }
                    else
                    {
                        foreach (var item in t.Result)
                        {
                            foreach (var value in item)
                            {
                                Debug.WriteLine("{0}: {1}", item.Key, value);
                            }
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ServiceBrowser serviceBrowser = new ServiceBrowser();
            serviceBrowser.ServiceAdded += onServiceAdded;
            serviceBrowser.ServiceRemoved += onServiceRemoved;
            serviceBrowser.ServiceChanged += onServiceChanged;

            serviceBrowser.StartBrowse("_openhab-server._tcp");
        }

        void onServiceChanged(object sender, ServiceAnnouncementEventArgs e)
        {
            printService('~', e.Announcement);
        }

        void onServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
            printService('-', e.Announcement);
        }

        void onServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            printService('+', e.Announcement);
        }

        static void printService(char startChar, ServiceAnnouncement service)
        {
            Console.WriteLine("{0} '{1}' on {2}", startChar, service.Instance, service.NetworkInterface.Name);
            Console.WriteLine("\tHost: {0} ({1})", service.Hostname, string.Join(", ", service.Addresses));
            Console.WriteLine("\tPort: {0}", service.Port);
            Console.WriteLine("\tTxt : [{0}]", string.Join(", ", service.Txt));
        }
    }
}
