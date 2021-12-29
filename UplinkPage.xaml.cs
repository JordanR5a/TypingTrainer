using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class UplinkPage : Page
    {
        public UplinkPage()
        {
            this.InitializeComponent();
        }

        private string SendGetRequest(string url)
        {
            //Create an HTTP client object
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri(url);

            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = httpClient.GetAsync(requestUri).AsTask().GetAwaiter().GetResult();
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = httpResponse.Content.ReadAsStringAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            return httpResponseBody;
        }

        private async void DisplayAddNovelDialog()
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ContentDialog unknownVIdeoDialog = new ContentDialog
            {
                Title = "Add Novel To Collection?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await unknownVIdeoDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary)
            {
                
            }
        }

        private bool AnyContentDialogOpen()
        {
            var openedpopups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in openedpopups)
            {
                if (popup.Child is ContentDialog) return true;
            }
            return false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            List<Book> books = JsonConvert.DeserializeObject<List<Book>>(SendGetRequest("http://localhost:8080/book"));
            CreateBookDisplay(books);

        }

        private void NovelBtn_Click(Object sender, RoutedEventArgs agrs)
        {
            if (!AnyContentDialogOpen()) DisplayAddNovelDialog();
        }

        private void CreateBookDisplay(List<Book> books)
        {
            foreach (Book book in books)
            {
                Button btn = new Button();
                btn.Name = book.BookID.ToString();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.Content = book.BookTitle;
                btn.FontSize = 40;
                btn.Margin = new Thickness(10);
                btn.CornerRadius = new CornerRadius(25);
                btn.Click += NovelBtn_Click;
                UplinkPageBookDisplay.Children.Add(btn);
            }
        }
    }
}
