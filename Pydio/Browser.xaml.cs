using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class Browser : Page
    {
        private ObservableCollection<Models.Node> Nodes = new ObservableCollection<Models.Node>();
        private Models.Server Server;
        private Pydio.API API;
        private List<string> History = new List<string>();
        private string WorkSpace = "";
        private SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private Models.File ClipboardContent = null;

        public Browser()
        {

            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new ContinuumNavigationTransitionInfo();



            theme.DefaultNavigationTransitionInfo = info;

            collection.Add(theme);
            this.Transitions = collection;

            this.InitializeComponent();

            systemNavigationManager.BackRequested += Go_Back;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

        }

        private void Go_Back(object sender, BackRequestedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Go_Back");
            e.Handled = true;

            if (WorkSpace.Equals(""))
            {
                System.Diagnostics.Debug.WriteLine("Going back to Main");

                if (this.Frame.CanGoBack)
                {
                    systemNavigationManager.BackRequested -= Go_Back;

                    this.Frame.GoBack();
                }
            }
            else {
                BrowseBack();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            using (var db = new Models.PydioContext())
            {

                Server = db.Servers.Where(b => b.ServerId == (int)e.Parameter).First();
                API = new Pydio.API(Server);
                BrowseRoot();
            }
        }




        private void Files_Click(object sender, TappedRoutedEventArgs e)
        {
            Models.Node CurrentNode = (Models.Node)Files.SelectedItem;

            if (CurrentNode.Folder)
            {
                string fileId = CurrentNode.Path;

                if (WorkSpace.Equals(""))
                {
                    WorkSpace = fileId;
                }
                else {
                    History.Add(fileId);
                }


                System.Diagnostics.Debug.WriteLine("Item clicked, path:" + CurrentNode.Path);
                Browse();


            }
        }


        private void BrowseBack()
        {
            if (History.Count > 0)
            {
                History.RemoveAt(History.Count - 1);
                Browse();
            }
            else {
                WorkSpace = "";
                //ResetClipboard();

                BrowseRoot();
            }

        }

        private async void BrowseRoot()
        {
            ProgressBar.Opacity = 100;
            List<Models.Node> nodes = await API.ListWorkspaces();
            Nodes = new ObservableCollection<Models.Node>(nodes);
            System.Diagnostics.Debug.WriteLine("Nodes:" + Nodes.Count);

            Files.ItemsSource = Nodes;

            CurrentFolder.Text = "Workspaces";
            ResetClipboard();
            ProgressBar.Opacity = 0;
        }

        private async void Browse()
        {
            if (!WorkSpace.Equals(""))
            {
                ProgressBar.Opacity = 100;
                string path = "";
                if (History.Count > 0)
                {
                    path = History.Last();
                    CurrentFolder.Text = path.Split('/').Last();
                }
                else {
                    CurrentFolder.Text = WorkSpace;
                }

                Nodes.Clear();


                List<Models.File> files = await API.Ls(WorkSpace, path);
                Files.ItemsSource = files;

                //Setting the title

                ProgressBar.Opacity = 0;
            }

        }

        private void ShowContextMenu(UIElement target, Point offset)
        {
            if (!WorkSpace.Equals(""))
            {
                Models.Node CurrentNode = (Models.Node)Files.SelectedItem;

                var MyFlyout = this.Resources["FileContextMenu"] as MenuFlyout;

                System.Diagnostics.Debug.WriteLine("MenuFlyout shown '{0}', '{1}' on item:{2}", target, offset, CurrentNode.Path);

                MyFlyout.ShowAt(target, offset);
            }
        }

        private void Files_Right_Click(object sender, RightTappedRoutedEventArgs e)
        {
            //var s = (FrameworkElement)sender;
            var d = ((FrameworkElement)e.OriginalSource).DataContext;
            System.Diagnostics.Debug.WriteLine("Selected Type :" + d.GetType());

            Models.Node node = (Models.Node)d;

            Files.SelectedItem = node;
            System.Diagnostics.Debug.WriteLine("Selected item :" + ((Models.Node)Files.SelectedItem).Label);

            ShowContextMenu(null, e.GetPosition(null));
        }


        private void Context_To_ClipBoard(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Context clipboard ");

            ClipboardContent = (Models.File)Files.SelectedItem;

            Color currentAccentColorHex = (Color)Application.Current.Resources["SystemAccentColor"];
            SolidColorBrush foreground = new SolidColorBrush(currentAccentColorHex);

            CommandClipBoard.Foreground = foreground;
            System.Diagnostics.Debug.WriteLine("Clipboard content:" + ClipboardContent.Label);

            ContextClipboardName.Text = ClipboardContent.Label;
            ContextClipboardMove.IsEnabled = true;
            ContextClipboardCopy.IsEnabled = true;
        }

        private void ContextClipboardMove_Click(object sender, RoutedEventArgs e)
        {
            CopyFile(true);
        }

        private async void CopyFile(bool Move)
        {
            ProgressBar.Opacity = 100;

            string MovePath = "";
            if (History.Count > 0)
            {
                MovePath = History.Last();
            }

            System.Diagnostics.Debug.WriteLine("Moving from:" + ClipboardContent.Path + " to: " + MovePath);

            bool result;
            if (Move)
            {
                result = await API.Move(WorkSpace, ClipboardContent.Path, MovePath);
            }
            else {
                result = await API.Copy(WorkSpace, ClipboardContent.Path, MovePath);
            }
            ProgressBar.Opacity = 0;

            if (result)
            {
                Browse();
            }
            else {
                var dialog = new MessageDialog("An error occured while moving the file.");
                await dialog.ShowAsync();
            }

            ResetClipboard();
        }

        private void ResetClipboard()
        {
            ClipboardContent = null;
            ContextClipboardName.Text = "Empty clipboard";
            ContextClipboardMove.IsEnabled = false;
            ContextClipboardCopy.IsEnabled = false;
            CommandClipBoard.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void ContextClipboardCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyFile(false);
        }

        private async void Context_Delete(object sender, RoutedEventArgs e)
        {

            Models.File File = (Models.File)Files.SelectedItem;

            var dialog = new Windows.UI.Popups.MessageDialog("Delete " + File.Label + " ?");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });


            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            System.Diagnostics.Debug.WriteLine("result:" + result.Id.ToString());


            if (result.Id.ToString().Equals("0"))
            {
                bool ApiResult = await API.Delete(WorkSpace, File.Path);
                if (ApiResult)
                {
                    Browse();
                }
                else {
                    var okDialog = new MessageDialog("An error occured while deleting the file.");
                    await okDialog.ShowAsync();
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Browse();
        }

        private async void Context_Rename(object sender, RoutedEventArgs e) {
            await RenameDialog.ShowAsync();
        }
    }
}
