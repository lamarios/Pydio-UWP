using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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


        public Settings()
        {
            this.InitializeComponent();
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

        private void refreshServers() {
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

        private void adjustColumns() {
            System.Diagnostics.Debug.WriteLine("Narrow:" + isNarrow+" editting ?:"+isEditting);

            if (isNarrow && isEditting)
            {
                DetailColumn.Width = new GridLength(1, GridUnitType.Star);
                MasterColumn.Width = new GridLength(0);

                // Register for hardware and software back request from the system
                SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
                systemNavigationManager.BackRequested += DetailPage_BackRequested;
                systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else if (isNarrow && !isEditting)
            {
                MasterColumn.Width = new GridLength(1, GridUnitType.Star);
                DetailColumn.Width = new GridLength(0);

                // Register for hardware and software back request from the system
                SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
                systemNavigationManager.BackRequested += DetailPage_BackRequested;
                systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
            else {
                DetailColumn.Width = new GridLength(1, GridUnitType.Star);
                MasterColumn.Width = new GridLength(1, GridUnitType.Auto);

                // Register for hardware and software back request from the system
                SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
                systemNavigationManager.BackRequested += DetailPage_BackRequested;
                systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void DetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            isEditting = false;
            adjustColumns();
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
            }
            else {
                title.Text = "Add Server";
                label.Text = "";
                serverAddress.Text = "";
                password.Password = "";
                userName.Text = "";
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

                if (Frame.CanGoBack)
                {
                    //Frame.GoBack();
                }

                refreshServers();

                var dialog = new Windows.UI.Popups.MessageDialog(dialogText);
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok") { Id = 0 });

                var result = await dialog.ShowAsync();
                isEditting = false;
                adjustColumns();
            }

        }

        private async void deleteServer(object sender, RoutedEventArgs e)
        {
            if (_lastSelectedItem != null) {
                var dialog = new Windows.UI.Popups.MessageDialog("Delete server ?");

                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });


                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsync();

                System.Diagnostics.Debug.WriteLine("result:" + result.Id.ToString());


                //if (result.Id ) {
                    var dialog2 = new Windows.UI.Popups.MessageDialog("Server deleted");
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Server deleted") { Id = 0 });

                    var result2 = await dialog.ShowAsync();
                    isEditting = false;
                    adjustColumns();
                //}
            }
        }
    }
}
