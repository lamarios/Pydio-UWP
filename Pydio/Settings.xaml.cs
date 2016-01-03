using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pydio
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private bool isNarrow, isEditting = false;
        private ObservableCollection<Models.Server> pydioServers = new ObservableCollection<Models.Server>();
        private Models.Server _lastSelectedItem;
        SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();

        public Settings()
        {
            System.Diagnostics.Debug.WriteLine("Loading Settings");

            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();



            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;
            this.InitializeComponent();


        }

        private void Go_Back(object sender, BackRequestedEventArgs e)
        {
            systemNavigationManager.BackRequested -= Go_Back;

            if (!isNarrow || (isNarrow && !isEditting)) {
                System.Diagnostics.Debug.WriteLine("Going back to Main");

                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                    return;

                // Navigate back if possible, and if the event has not 
                // already been handled .
                if (rootFrame.CanGoBack && e.Handled == false)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                }
            }
            else {
                System.Diagnostics.Debug.WriteLine("Going back to Master");

                e.Handled = true;
                isEditting = false;
                adjustColumns();
                systemNavigationManager.BackRequested += Go_Back;

            }

        }

        

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (Models.Server)e.ClickedItem;
            _lastSelectedItem = clickedItem;

            setServer();

        }




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            refreshServers();

        }

        private void refreshServers()
        {
            using (var db = new Models.PydioContext())
            {
                pydioServers = new ObservableCollection<Models.Server>(db.Servers.ToList());
                MasterListView.ItemsSource = pydioServers;
                System.Diagnostics.Debug.WriteLine("Servers:" + pydioServers.Count);

                UpdateForVisualState(AdaptiveStates.CurrentState);
            }
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            // Assure we are displaying the correct item. This is necessary in certain adaptive cases.
            MasterListView.SelectedItem = _lastSelectedItem;
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            isNarrow = newState == NarrowState;

            adjustColumns();
        }

        private void adjustColumns()
        {
            System.Diagnostics.Debug.WriteLine("Narrow:" + isNarrow + " editting ?:" + isEditting);

            if (isNarrow && isEditting)
            {
                DetailColumn.Width = new GridLength(1, GridUnitType.Star);
                MasterColumn.Width = new GridLength(0);

            }
            else if (isNarrow && !isEditting)
            {
                MasterColumn.Width = new GridLength(1, GridUnitType.Star);
                DetailColumn.Width = new GridLength(0);

            }
            else {
                DetailColumn.Width = new GridLength(1, GridUnitType.Star);
                MasterColumn.Width = new GridLength(300, GridUnitType.Pixel);
            }
        }



        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            _lastSelectedItem = null;

            setServer();
        }

        private void setServer()
        {
            isEditting = true;

            if (_lastSelectedItem != null)
            {
                label.Text = _lastSelectedItem.Name;
                serverAddress.Text = _lastSelectedItem.Url;
                password.Password = _lastSelectedItem.Password;
                userName.Text = _lastSelectedItem.Username;
                title.Text = "Edit: " + _lastSelectedItem.Name;
                delete.Visibility = Visibility.Visible;

            }
            else {
                title.Text = "Add Server";
                label.Text = "";
                serverAddress.Text = "";
                password.Password = "";
                userName.Text = "";
                delete.Visibility = Visibility.Collapsed;

            }



            adjustColumns();


        }

        private void setLabel(object sender, KeyRoutedEventArgs e)
        {
            label.Text = serverAddress.Text.Replace("http://", "");
        }

        private async void saveServer(object sender, RoutedEventArgs e)
        {
            using (var db = new Models.PydioContext())
            {

                Models.Server server = new Models.Server();
                server.Name = label.Text.Trim();
                server.Url = serverAddress.Text.Trim();
                server.Password = password.Password;
                server.Username = userName.Text;

                string dialogText;

                if (_lastSelectedItem != null)
                {
                    server.ServerId = _lastSelectedItem.ServerId;
                    _lastSelectedItem = server;
                    db.Servers.Update(_lastSelectedItem);
                    dialogText = "Server saved !";
                }
                else {
                    db.Servers.Add(server);
                    dialogText = "Server added !";
                }
                db.SaveChanges();



                refreshServers();

                Notify("Server Saved");
                //var dialog = new Windows.UI.Popups.MessageDialog(dialogText);
                //dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok") { Id = 0 });

                //var result = await dialog.ShowAsync();
                isEditting = false;
                adjustColumns();
            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            delete.Visibility = Visibility.Collapsed;

            systemNavigationManager.BackRequested += Go_Back;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

        }

        private async void delete_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSelectedItem != null)
            {
                using (var db = new Models.PydioContext())
                {
                    var dialog = new Windows.UI.Popups.MessageDialog("Delete server ?");

                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });


                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsync();

                    System.Diagnostics.Debug.WriteLine("result:" + result.Id.ToString());


                    if (result.Id.ToString().Equals("0"))
                    {
                        db.Servers.Remove(_lastSelectedItem);
                        db.SaveChanges();
                        _lastSelectedItem = null;

                        setServer();
                        isEditting = false;
                        adjustColumns();
                        refreshServers();
                        Notify("Server Deleted");

                    }
                }
            }
        }

        private void Notify(string Message) {
            Notification.Visibility = Visibility.Visible;
            NotificationText.Text = Message;
            NotificationAnimation.Begin();

        }
    }
}
