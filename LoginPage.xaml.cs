using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TypingTrainer.DatabaseObjects;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace TypingTrainer
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ApiManager.CheckValidation(username.Text, password.Password))
            {
                App.userName = username.Text;
                App.password = password.Password;
                App.PageNavigation.Back(Frame);
            }
        }
    }
}
