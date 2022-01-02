using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TypingTrainer.ApplicationObjects;
using TypingTrainer.DatabaseObjects;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace TypingTrainer
{

    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            App.PageNavigation = new PageNavigation(typeof(MainPage));
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void MainPageTrainerPageBtn_Click(object sender, RoutedEventArgs e)
        {
            App.PageNavigation.Navigate(Frame, typeof(TrainerPage));
        }

        private void MainPageUplinkPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ApiManager.SystemOnline()) App.PageNavigation.Navigate(Frame, typeof(UplinkPage));
        }
    }
}
