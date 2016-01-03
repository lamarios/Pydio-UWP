using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

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
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;


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
