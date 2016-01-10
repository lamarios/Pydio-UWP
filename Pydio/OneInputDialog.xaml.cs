using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Pydio
{
    public sealed partial class OneInputDialog : ContentDialog
    {

        private Models.File File { get; set; }
        public const int TYPE_RENAME = 0, TYPE_CREATE = 1;
        public int type;
        private Pydio.API API;
        private string Path, Workspace;

        public OneInputDialog(string Workspace, string Path, Models.Server Server)
        {
            this.InitializeComponent();

            API = new Pydio.API(Server);
            this.Path = Path;
            this.type = TYPE_CREATE;
            this.Workspace = Workspace;
            this.Title = "Create new folder";
            Input.PlaceholderText = "New folder name";
        }

        public OneInputDialog(Models.File FileParam, Models.Server Server)
        {
            this.InitializeComponent();

            API = new Pydio.API(Server);
            this.File = FileParam;
            this.type = TYPE_RENAME;


            this.Title = "Rename " + FileParam.Label;
            Input.PlaceholderText = "New file name";
            Input.Text = FileParam.Label;
            Input.SelectionStart = 0;

            Input.SelectionLength = File.Label.Split('.')[0].Length;

        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            switch (type)
            {
                case TYPE_CREATE:
                    CreateFile();
                    break;
                case TYPE_RENAME:
                    await RenameFile();
                    break;
            }
        }

        private async Task RenameFile()
        {
            try
            {
                bool result = await API.Rename(File, Input.Text);
            }
            catch (Exceptions.PydioException e)
            {
                var okDialog = new MessageDialog(e.Message, "Error");
                await okDialog.ShowAsync();
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void CreateFile()
        {
            try
            {
                bool result = await API.MkDir(Workspace, Path, Input.Text);
            }
            catch (Exceptions.PydioException e)
            {
                var okDialog = new MessageDialog(e.Message, "Error");
                await okDialog.ShowAsync();
            }
        }

    }
}
