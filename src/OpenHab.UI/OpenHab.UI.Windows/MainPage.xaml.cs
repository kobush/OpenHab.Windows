using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OpenHab.Client;
using Page = Windows.UI.Xaml.Controls.Page;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OpenHab.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new OpenHabRestClient(new Uri("http://192.168.1.2:8080"));

            var cts = new System.Threading.CancellationTokenSource();

            var items = await client.GetItems(cts.Token);

            var groupItem = await client.GetItem(items.First(i => i.Type == ItemType.GroupItem), cts.Token);
            var switchItem = await client.GetItem(items.First(i => i.Type == ItemType.SwitchItem), cts.Token);
            var rollershutterItem = await client.GetItem(items.First(i => i.Type == ItemType.RollershutterItem), cts.Token);
            var dimmerItem = await client.GetItem(items.First(i => i.Type == ItemType.DimmerItem), cts.Token);
            var colorItem = await client.GetItem(items.First(i => i.Type == ItemType.ColorItem), cts.Token);
            var numberItem = await client.GetItem(items.First(i => i.Type == ItemType.NumberItem), cts.Token);
            var stringItem = await client.GetItem(items.First(i => i.Type == ItemType.StringItem), cts.Token);
            var contactItem = await client.GetItem(items.First(i => i.Type == ItemType.ContactItem), cts.Token);
            var dateTimeItem = await client.GetItem(items.First(i => i.Type == ItemType.DateTimeItem), cts.Token);

            var sitemaps = await client.GetSitemaps(cts.Token);

            var page = await client.GetPage(sitemaps.First().Homepage, cts.Token);
        }
    }
}
