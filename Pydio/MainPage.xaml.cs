using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pydio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Models.Server> pydioServers = new ObservableCollection<Models.Server>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void showSettings(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            using (var db = new Models.PydioContext())
            {
                pydioServers = new ObservableCollection<Models.Server>(db.Servers.ToList());
                Servers.ItemsSource = pydioServers;
                System.Diagnostics.Debug.WriteLine("Servers:"+ pydioServers.Count);
            }
        }

        

        private void ServerClicked(object sender, TappedRoutedEventArgs e)
        {
            Models.Server server = (Models.Server) Servers.SelectedItem;
            System.Diagnostics.Debug.WriteLine("Servers:" + server.Name);

            Pydio.API API = new Pydio.API(server);
            API.Ls("yo", "");

        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
