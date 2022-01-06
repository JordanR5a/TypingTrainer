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
        private async void DisplayLocalNovelDialog(Object sender)
        {
            ContentDialog localNovelDialog = new ContentDialog
            {
                Title = "Export Novel",
                Content = "Do you wish to share this novel to the world?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await localNovelDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary) ExportNovel(sender);
        }

        private void ExportNovel(Object sender)
        {
            string bookId = (sender as Button).Name;
            Book book = DatabaseManager.GetBook(int.Parse(bookId));

            List<Chapter> chapters = DatabaseManager.GetChapters(int.Parse(bookId));

            ApiManager.CreateBook(book, chapters);
        }

        private async void DisplayAddNewNovelDialog(Object sender)
        {
            ContentDialog newNovelDialog = new ContentDialog
            {
                Title = "Add Novel To Collection",
                Content = "Decide whether to import this novel into your own library.",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await newNovelDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary) ImportNovel(sender);
        }

        private async void DisplayPsuedoLocalNovelDialog(Object sender)
        {
            ContentDialog localNovelDialog = new ContentDialog
            {
                Title = "Update Local Novel",
                Content = "This novel is already present in your library, do you wish to override and replace it with its global version if it exists?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await localNovelDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary) ImportNovel(sender);
        }

        private void ImportNovel(Object sender)
        {
            string bookId = (sender as Button).Name;
            List<Chapter> chapters = ApiManager.GetChaptersByBookId(int.Parse(bookId));

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
            List<Book> local = DatabaseManager.GetBooks();
            List<Book> external = ApiManager.GetAllBooks();

            bookGallery = local.Where(x => external.TrueForAll(k => x.BookTitle != k.BookTitle)).Select(x => new LocalBook(x.BookID, x.BookTitle, LocalBook.BookType.LOCAL)).ToList();
            bookGallery.AddRange(local.Where(x => !external.TrueForAll(k => x.BookTitle != k.BookTitle)).Select(x => new LocalBook(x.BookID, x.BookTitle, LocalBook.BookType.PSUEDO_LOCAL)));
            bookGallery.AddRange(external.Where(x => local.TrueForAll(k => x.BookTitle != k.BookTitle)).Select(x => new LocalBook(x.BookID, x.BookTitle, LocalBook.BookType.EXTERNAL)));

            CreateBookDisplay(bookGallery);
        }

        private void NovelBtn_Click(Object sender, RoutedEventArgs agrs)
        {
            if (!AnyContentDialogOpen()) DisplayAddNewNovelDialog(sender);
        }

        private void PsuedoLocalNovelBtn_Click(Object sender, RoutedEventArgs args)
        {
            if (!AnyContentDialogOpen()) DisplayPsuedoLocalNovelDialog(sender);
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

                switch (book.Type)
                {
                    case LocalBook.BookType.LOCAL:
                        btn.Click += LocalNovelBtn_Click;
                        break;
                    case LocalBook.BookType.PSUEDO_LOCAL:
                        btn.Click += PsuedoLocalNovelBtn_Click;
                        break;
                    case LocalBook.BookType.EXTERNAL:
                        btn.Click += NovelBtn_Click;
                        break;
                }

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
