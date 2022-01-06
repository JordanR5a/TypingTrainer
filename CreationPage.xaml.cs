using Microsoft.UI.Xaml.Controls;
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
    public sealed partial class CreationPage : Page
    {
        //TODO: Subscriptions (Experation dates and automatic reposting)
        public ViewModel viewModel { get; set; }
        private Book book;
        private List<Chapter> chapters;
        public CreationPage()
        {
            this.InitializeComponent();

            viewModel = new ViewModel();

            this.DataContext = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            book = new Book();
            string availableId;
            do
            {
                availableId = new Random().Next(int.MaxValue).ToString();
            }
            while (DatabaseManager.BookExists(availableId));
            book.BookID = int.Parse(availableId);

            chapters = new List<Chapter>();
        }

        private void CreationPageBackBtn_Click(object sender, RoutedEventArgs e)
        {
            App.PageNavigation.Back(Frame);
        }

        private void CreationPageAddChapter_Click(object sender, RoutedEventArgs e)
        {
            if (!AnyContentDialogOpen()) DisplayAddChapterDialog(null);
        }

        private void CreationPageEditChapter_Click(object sender, RoutedEventArgs e)
        {
            Chapter chapter = chapters.Find(x => x.ChapterID.Equals(int.Parse(((Button)sender).Name)));
            if (!AnyContentDialogOpen()) DisplayAddChapterDialog(chapter);
        }

        private void CreateChapterDisplay()
        {
            CreationPageChapterDisplay.Children.Clear();
            foreach (Chapter chapter in chapters)
            {
                Button chapterBtn = new Button();
                chapterBtn.Click += CreationPageEditChapter_Click;
                chapterBtn.Name = chapter.ChapterID.ToString();
                chapterBtn.Content = "Chapter " + chapter.ChapterNumber;
                chapterBtn.Width = 250;
                chapterBtn.FontSize = 35;
                CreationPageChapterDisplay.Children.Add(chapterBtn);
            }

            Button addChapterBtn = new Button();
            addChapterBtn.Click += CreationPageAddChapter_Click;
            addChapterBtn.Content = "New Chapter";
            addChapterBtn.Width = 250;
            addChapterBtn.FontSize = 35;
            addChapterBtn.Focus(FocusState.Programmatic);
            CreationPageChapterDisplay.Children.Add(addChapterBtn);
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            sender.Text = new String(sender.Text.Where(char.IsDigit).ToArray());
        }

        private async void DisplayFinalizeAddChapterDialog(Chapter chapter)
        {
            TextBox box = new TextBox();
            box.TextChanging += TextBox_OnTextChanging;

            ContentDialog addChapterDialog = new ContentDialog
            {
                Title = "Chapter Number",
                Content = box,
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Back"
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await addChapterDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary && chapters.TrueForAll(x => !x.ChapterNumber.Equals(int.Parse(box.Text))))
            {
                if (chapter.ChapterID == -1)
                {
                    string availableId;
                    do
                    {
                        availableId = new Random().Next(int.MaxValue).ToString();
                    }
                    while (DatabaseManager.ChapterExists(availableId) || !chapters.TrueForAll(x => x.ChapterID != int.Parse(availableId)));
                    chapter.ChapterID = int.Parse(availableId);
                }
                chapter.ChapterText = chapter.ChapterText;
                chapter.ChapterNumber = int.Parse(box.Text);
                if (chapters.TrueForAll(x => !x.ChapterID.Equals(chapter.ChapterID))) chapters.Add(chapter);
                CreateChapterDisplay();
            }
            else if (result == ContentDialogResult.Secondary) DisplayAddChapterDialog(chapter);
        }

        private async void DisplayAddChapterDialog(Chapter chapter)
        {
            TextBox box = new TextBox();
            box.Height = 500;
            box.Width = 400;
            box.AllowFocusOnInteraction = true;
            box.TextWrapping = TextWrapping.Wrap;
            if (chapter == null)
            {
                chapter = new Chapter();
                chapter.ChapterID = -1;
            }
            if (chapter.ChapterText != null && chapter.ChapterText != "") box.Text = chapter.ChapterText;
            ContentDialog addChapterDialog = new ContentDialog
            {
                Title = "New Chapter",
                Content = box,
                PrimaryButtonText = "Continue",
                CloseButtonText = "Close",
            };

            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await addChapterDialog.ShowAsync();
            }
            catch (Exception e) { return; }

            if (result == ContentDialogResult.Primary)
            {
                chapter.ChapterText = box.Text.Trim();
                DisplayFinalizeAddChapterDialog(chapter);
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

        private void CreationPageSaveNovel_Click(object sender, RoutedEventArgs e)
        {
            if (chapters.Count <= 0 || CreationPageNovelTitleTextBox.Text.Trim().Equals("")) return;
            book.BookTitle = CreationPageNovelTitleTextBox.Text.Trim();
            DatabaseManager.CreateBook(book);
            foreach (Chapter chapter in chapters) DatabaseManager.CreateChapter(chapter, book.BookID.ToString());
            App.PageNavigation.Back(Frame);
        }
    }
}
