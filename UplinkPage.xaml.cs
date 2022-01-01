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
        private List<LocalBook> bookGallery;
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

        private async void DisplayAddNewNovelDialog(Object sender)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ContentDialog unknownVIdeoDialog = new ContentDialog
            {
                Title = "Add Novel To Collection?",
                Content = "Decide whether to import this novel into your own library.",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await unknownVIdeoDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary) ImportNovel(sender);
        }

        private async void DisplayLocalNovelDialog(Object sender)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ContentDialog unknownVIdeoDialog = new ContentDialog
            {
                Title = "Update Local Novel?",
                Content = "This novel is already present in your library, do you wish to override and replace it with its global version?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await unknownVIdeoDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary) ImportNovel(sender);
        }

        private void ImportNovel(Object sender)
        {
            string bookId = (sender as Button).Name;
            var relevantChapters = SendGetRequest("http://localhost:8080/book/" + bookId);
            List<Chapter> chapters = JsonConvert.DeserializeObject<List<Chapter>>(relevantChapters);

            if (!DatabaseManager.BookExists(bookId)) DatabaseManager.CreateBook(bookGallery.Find(x => x.BookID.ToString().Equals(bookId)));
            foreach (Chapter chapter in chapters)
            {
                DatabaseManager.CreateChapter(chapter, bookId);
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
            bookGallery = DatabaseManager.GetBooks().Select(x => new LocalBook(x.BookID, x.BookTitle, true)).ToList();
            bookGallery.AddRange(JsonConvert.DeserializeObject<List<Book>>(SendGetRequest("http://localhost:8080/book")).Where(x => bookGallery.TrueForAll(j => x.BookID != j.BookID)).Select(x => new LocalBook(x.BookID, x.BookTitle, false)));
            CreateBookDisplay(bookGallery);

        }

        private void NovelBtn_Click(Object sender, RoutedEventArgs agrs)
        {
            if (!AnyContentDialogOpen()) DisplayAddNewNovelDialog(sender);
        }

        private void LocalNovelBtn_Click(Object sender, RoutedEventArgs args)
        {
            if (!AnyContentDialogOpen()) DisplayLocalNovelDialog(sender);
        }

        private void CreateBookDisplay(List<LocalBook> books)
        {
            foreach (LocalBook book in books)
            {
                Button btn = new Button();
                btn.Name = book.BookID.ToString();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.Content = book.BookTitle;
                btn.FontSize = 40;
                btn.Margin = new Thickness(10);
                btn.CornerRadius = new CornerRadius(25);
                if (book.IsLocal) btn.Click += LocalNovelBtn_Click;
                else btn.Click += NovelBtn_Click;
                UplinkPageBookDisplay.Children.Add(btn);
            }
        }

        private void UplinkPageBackBtn_Click(object sender, RoutedEventArgs e)
        {
            App.PageNavigation.Back(Frame);
        }

        private void UplinkPageCreateBtn_Click(object sender, RoutedEventArgs e)
        {
            App.PageNavigation.Navigate(Frame, typeof(CreationPage));
        }
    }
}
