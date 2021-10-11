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
        static readonly int DISPLAY_SIZE = 200;
        static string CURRENT_NOVEL = "Free The Darkness";

        int startText;
        bool rawFormat;
        Trainer trainer;

        DispatcherTimer timer;
        List<int> WPMData;
        int totalSeconds;
        int highest;
        int lowest;

        public MainPage()
        {
            this.InitializeComponent();
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            Window.Current.CoreWindow.PointerCursor = null;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;

            rawFormat = false;
            StartChapter(CURRENT_NOVEL);
        }

        void StartChapter(string novelName)
        {
            WPMData = new List<int>();
            totalSeconds = 0;
            highest = int.MinValue;
            lowest = int.MaxValue;

            trainer = new Trainer(novelName);

            startText = trainer.Place;
            UpdateMainDisplay();
        }

        private void TimerTick(object o, Object e)
        {
            totalSeconds++;
            if (totalSeconds % 60 == 0)
            {
                if (trainer.WordsTyped > 20) WPMData.Add(trainer.WordsTyped);
                trainer.WordsTyped = 0;
                setLimits();
            }

        }

        private void setLimits()
        {
            foreach (int num in WPMData)
            {
                if (num > highest) highest = num;
                if (num < lowest) lowest = num;
            }
        }

        async Task PlaySound(string filename, Boolean enabled)
        {
            if (!enabled) return;
            var element = new MediaElement();
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Resources");
            var file = await folder.GetFileAsync(filename);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            element.SetSource(stream, "");
            element.Play();
        }

        async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {


            if (e.VirtualKey == VirtualKey.Escape && !AnyContentDialogOpen()) ChangeNovelPrompt();
            else if (e.VirtualKey == VirtualKey.Tab && !AnyContentDialogOpen()) UpdateWPMDisplay();
            else if (e.VirtualKey == VirtualKey.CapitalLock && !AnyContentDialogOpen())
            {
                rawFormat = rawFormat ? false : true;
                if (WPMDisplay.Opacity > 0) UpdateWPMDisplay();
            }
            else if (e.VirtualKey == VirtualKey.Right && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (trainer.NextChapter())
                {
                    startText = trainer.Place;
                    UpdateMainDisplay();
                    await PlaySound("shortMove.wav", true);
                }
                else await PlaySound("error.wav", true);
            }
            else if (e.VirtualKey == VirtualKey.Left && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (trainer.PreviousChapter())
                {
                    startText = trainer.Place;
                    UpdateMainDisplay();
                    await PlaySound("shortMove.wav", true);
                }
                else await PlaySound("error.wav", true);
            }
            else if (e.VirtualKey == VirtualKey.Down && !AnyContentDialogOpen()) MoveDown(1, "return.mp3");
            else if (e.VirtualKey == VirtualKey.PageDown && !AnyContentDialogOpen()) MoveDown(5, "shortMove.wav");
            else if (e.VirtualKey == VirtualKey.Up && !AnyContentDialogOpen()) MoveUp(2, "return.mp3");
            else if (e.VirtualKey == VirtualKey.PageUp && !AnyContentDialogOpen()) MoveUp(6, "shortMove.wav");
            else if (e.VirtualKey == VirtualKey.Enter && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (trainer.Space())
                {
                    await PlaySound("returnBell.wav", false);
                    startText = trainer.Place;
                }
                else await PlaySound("error.wav", false);
            }
            else if (e.VirtualKey == VirtualKey.Space && !EndOfChapter() && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (trainer.Space()) await PlaySound("space.mp3", false);
                else await PlaySound("error.wav", false);
            }
            else if (e.VirtualKey == VirtualKey.Back && !EndOfChapter() && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (trainer.BackSpace()) await PlaySound("backspace.mp3", false);
                else await PlaySound("error.wav", false);
            }
            else if (InputParser.GetCharEquivalent(e) != ' ' && !EndOfChapter() && !AnyContentDialogOpen())
            {
                WPMDisplay.Opacity = 0;
                if (!timer.IsEnabled) timer.Start();
                if (trainer.Check(InputParser.GetCharEquivalent(e))) await PlaySound("erikaTap.mp3", false);
                else await PlaySound("error.wav", false);
            }

            UpdateMainDisplay();
        }

        async void MoveDown(int change, string goodSound)
        {
            if (trainer.Forward(change, rawFormat))
            {
                timer.Stop();
                await PlaySound(goodSound, false);
                startText = trainer.Place;
                trainer.Restart();
                UpdateMainDisplay();
            }
            else await PlaySound("error.wav", true);
        }

        async void MoveUp(int change, string goodSound)
        {
            if (trainer.Rewind(change, rawFormat))
            {
                timer.Stop();
                await PlaySound(goodSound, false);
                startText = trainer.Place;
                trainer.Restart();
                UpdateMainDisplay();
            }
            else await PlaySound("error.wav", true);
        }

        async void ChangeNovelPrompt()
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            NewNovel contentDialog = new NewNovel();
            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    string newNovel = contentDialog.title.Trim();
                    StartChapter(newNovel);
                    CURRENT_NOVEL = newNovel;
                    await PlaySound("shortMove.wav", true);
                }
                catch (DirectoryNotFoundException error)
                {
                    await PlaySound("error.wav", true);
                }
            }
            Window.Current.CoreWindow.PointerCursor = null;
        }

        bool AnyContentDialogOpen()
        {
            var openedpopups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in openedpopups)
            {
                if (popup.Child is ContentDialog) return true;
            }
            return false;
        }

        void UpdateWPMDisplay()
        {
            timer.Stop();
            WPMDisplay.Opacity = 0.8;
            double average = 0;
            if (WPMData.Count > 0) average = WPMData.Average();
            WPMDisplay.Text = $"Time: {new TimeSpan(0, 0, totalSeconds)}\n" +
                              $"WPM: {(WPMData.Count > 0 ? string.Format("{0:F2}", average) : "No data")}\n" +
                              $"High: {(WPMData.Count > 0 ? highest.ToString() : "No data")}\n" +
                              $"Low: {(WPMData.Count > 0 ? lowest.ToString() : "No data")}\n" +
                              $"Mode: {(rawFormat ? "Raw Format" : "Formatted")}";
        }

        void UpdateMainDisplay()
        {
            Display.Inlines.Clear();
            if (EndOfChapter())
            {
                UpdateWPMDisplay();
                CurrentChapterDisplay.Opacity = 0;
                Display.Text = "End of Chapter\n";
                Display.FontSize = 72;
                Display.Inlines.Add(new Bold());
                Display.TextAlignment = TextAlignment.Center;

                Run instructions = new Run();
                instructions.Text = "Please use the left or right arrow keys to change your currently selected chapter; or press Escape to switch your current novel.";
                instructions.FontSize = 45;
                Display.Inlines.Add(instructions);
            }
            else
            {
                CurrentChapterDisplay.Text = "Chapter " + trainer.CurrentChapterNumber;
                CurrentChapterDisplay.Opacity = 0.7;
                Display.FontSize = 34;
                Display.TextAlignment = TextAlignment.Left;

                int dynamicDisplaySize;
                if (startText + DISPLAY_SIZE > trainer.Sections.Length) dynamicDisplaySize = trainer.Sections.Length;
                else dynamicDisplaySize = startText + DISPLAY_SIZE;

                bool inQuotes = false;
                for (int loc = startText; loc < dynamicDisplaySize; loc++)
                {
                    Section currentSection = trainer.Sections[loc];
                    if (currentSection.ToString().Contains(Trainer.START_QUOTATION)) inQuotes = true;
                    if (currentSection.ToString().Contains(Trainer.END_QUOTATION)) inQuotes = false;
                    for (int innerLoc = 0; innerLoc < currentSection.Word.Length; innerLoc++)
                    {
                        Unit focus = currentSection.Word[innerLoc];
                        string spacing = BuildSpacing(loc, innerLoc, inQuotes);
                        if (trainer.Place == loc && trainer.Focus == innerLoc)
                        {
                            Run focusText = BuildRun(focus, spacing, Colors.LightGray);
                            Display.Inlines.Add(focusText);
                        }
                        else if (focus.Typed && focus.Correct)
                        {
                            Run typedText = BuildRun(focus, spacing, Colors.DarkGray);
                            Display.Inlines.Add(typedText);
                        }
                        else if (focus.Typed && !focus.Correct)
                        {
                            Run incorrectText = BuildRun(focus, spacing, Colors.DarkRed);
                            Display.Inlines.Add(incorrectText);
                        }
                        else
                        {
                            Run untypedText = BuildRun(focus, spacing, Colors.White);
                            Display.Inlines.Add(untypedText);
                        }
                    }
                    if (rawFormat)
                    {
                        Run modifierText = new Run();
                        modifierText.Text = currentSection.Modifier;
                        Display.Inlines.Add(modifierText);
                    }
                }
            }
        }

        string BuildSpacing(int loc, int innerLoc, bool inQuotes)
        {
            if (rawFormat) return "";

            string spacing = "";
            Section currentSection = trainer.Sections[loc];
            Unit focus = currentSection.Word[innerLoc];
            if (loc < trainer.Sections.Length - 1)
            {
                if (focus.Equals(Trainer.END_QUOTATION) && trainer.Sections[loc + 1].ToString().First().Equals(Trainer.START_QUOTATION)) spacing = "\n\n";
                else if (inQuotes && innerLoc == currentSection.Word.Length - 1) spacing = " ";
                else if (innerLoc == currentSection.Word.Length - 1)
                {
                    if (focus.Equals('.') || focus.Equals('!') || focus.Equals('?')) spacing = "\n\n";
                    else spacing = " ";
                }
            }
            return spacing;
        }

        string BuildText(Unit focus, string spacing)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(focus);
            builder.Append(spacing);

            return builder.ToString();
        }

        Run BuildRun(Unit focus, string spacing, Color color)
        {
            Run run = new Run();
            run.Text = BuildText(focus, spacing);
            run.Foreground = new SolidColorBrush(color);

            return run;
        }

        bool EndOfChapter()
        {
            if (trainer.Place >= trainer.Sections.Length) return true;
            else return false;
        }

    }
}
