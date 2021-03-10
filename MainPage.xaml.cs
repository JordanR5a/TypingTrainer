using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TypingTrainer
{

    public sealed partial class MainPage : Page
    {
        static readonly int DISPLAY_SIZE = 200;
        static string CURRENT_NOVEL = "Everything_Will_Be_My_Way!";

        int currentChapter;
        int startText;
        Trainer trainer;
        DispatcherTimer timer;
        List<int> WPMData;
        int totalSeconds;
        int secondsPast;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            Window.Current.CoreWindow.PointerCursor = null;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;

            currentChapter = 1;
            StartChapter(@"Data\" + CURRENT_NOVEL + @"\" + currentChapter);

        }

        void StartChapter(string filename)
        {
            WPMData = new List<int>();
            secondsPast = 0;
            totalSeconds = secondsPast;

            trainer = new Trainer(filename + ".html");

            startText = trainer.Place;
            updateMainDisplay();
            CurrentChapterDisplay.Text = "Current Chapter: " + currentChapter;
        }

        private void TimerTick(object o, Object e)
        {
            if (secondsPast >= 60)
            {
                totalSeconds += secondsPast;
                secondsPast = 0;

                WPMData.Add(trainer.WordsTyped);
                trainer.WordsTyped = 0;
            }
            else secondsPast++;

        }

        async Task PlaySound(string filename)
        {
            if (filename != "error.wav") return;
            var element = new MediaElement();
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Resources");
            var file = await folder.GetFileAsync(filename);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            element.SetSource(stream, "");
            element.Play();
        }

        async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            

            if (e.VirtualKey == VirtualKey.Escape && !AnyContentDialogOpen()) changeNovelPrompt();
            else if (e.VirtualKey == VirtualKey.CapitalLock && !AnyContentDialogOpen()) updateWPMDisplay();
            else if (e.VirtualKey == VirtualKey.Right && !AnyContentDialogOpen())
            {
                try
                {
                    StartChapter(@"Data\" + CURRENT_NOVEL + @"\" + ++currentChapter);
                }
                catch (FileNotFoundException error)
                {
                    currentChapter--;
                    await PlaySound("error.wav");
                }
            }
            else if (e.VirtualKey == VirtualKey.Left && !AnyContentDialogOpen())
            {
                try
                {
                    StartChapter(@"Data\" + CURRENT_NOVEL + @"\" + --currentChapter);
                }
                catch (FileNotFoundException error)
                {
                    currentChapter++;
                    await PlaySound("error.wav");
                }
            }
            else if (e.VirtualKey == VirtualKey.Down && !AnyContentDialogOpen()) moveDown(1, "return.mp3");
            else if (e.VirtualKey == VirtualKey.PageDown && !AnyContentDialogOpen()) moveDown(5, "shortMove.wav");
            else if (e.VirtualKey == VirtualKey.Up && !AnyContentDialogOpen()) moveUp(2, "return.mp3");
            else if (e.VirtualKey == VirtualKey.PageUp && !AnyContentDialogOpen()) moveUp(6, "shortMove.wav");
            else if (e.VirtualKey == VirtualKey.Enter && !AnyContentDialogOpen())
            {
                if (trainer.Space())
                {
                    await PlaySound("returnBell.wav");
                    startText = trainer.Place;
                }
                else await PlaySound("error.wav");
            }
            else if (e.VirtualKey == VirtualKey.Space && !EndOfChapter() && !AnyContentDialogOpen())
            {
                if (trainer.Space()) await PlaySound("space.mp3");
                else await PlaySound("error.wav");
            }
            else if (e.VirtualKey == VirtualKey.Back && !EndOfChapter() && !AnyContentDialogOpen())
            {
                if (trainer.BackSpace()) await PlaySound("backspace.mp3");
                else await PlaySound("error.wav");
            }
            else if (InputParser.GetCharEquivalent(e) != '~' && !EndOfChapter() && !AnyContentDialogOpen())
            {
                timer.Start();
                WPMDisplay.Opacity = 0;
                if (trainer.Check(InputParser.GetCharEquivalent(e))) await PlaySound("erikaTap.mp3");
                else await PlaySound("error.wav");
            }

            updateMainDisplay();
        }

        async void moveDown(int change, string goodSound)
        {
            if (trainer.Forward(change))
            {
                timer.Stop();
                await PlaySound(goodSound);
                startText = trainer.Place;
                trainer.Restart();
                updateMainDisplay();
            }
            else await PlaySound("error.wav");
        }

        async void moveUp(int change, string goodSound)
        {
            if (trainer.Rewind(change))
            {
                timer.Stop();
                //await PlaySound(goodSound);
                startText = trainer.Place;
                trainer.Restart();
                updateMainDisplay();
            }
            else await PlaySound("error.wav");
        }

        async void changeNovelPrompt()
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            NewNovel contentDialog = new NewNovel();
            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int pastChapter = currentChapter;
                try
                {
                    currentChapter = 1;
                    StartChapter(@"Data\" + contentDialog.title + @"\" + currentChapter);
                    CURRENT_NOVEL = contentDialog.title;
                }
                catch (DirectoryNotFoundException error)
                {
                    await PlaySound("error.wav");
                    currentChapter = pastChapter;
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

        async void updateWPMDisplay()
        {
            if (WPMData.Count > 0)
            {
                timer.Stop();
                WPMDisplay.Opacity = 0.8;
                double average = WPMData.Average();
                WPMDisplay.Text = $"Seconds: {totalSeconds}\n WPM: {string.Format("{0:F2}", average)}";
            }
            else await PlaySound("error.wav");
        }

        void updateMainDisplay()
        {
            Display.Inlines.Clear();
            if (EndOfChapter())
            {
                CurrentChapterDisplay.Opacity = 0;
                Display.Text = "End of Chapter\n";
                Display.FontSize = 72;
                Display.Inlines.Add(new Bold());
                Display.TextAlignment = TextAlignment.Center;

                Run instructions = new Run();
                instructions.Text = "Please use the left or right arrow keys to change your currently selected chapter; or press enter to switch your current novel.";
                instructions.FontSize = 45;
                Display.Inlines.Add(instructions);
            }
            else
            {
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
                    if (currentSection.ToString().First().Equals(Trainer.START_QUOTATION)) inQuotes = true;
                    if (currentSection.ToString().Last().Equals(Trainer.END_QUOTATION)) inQuotes = false;
                    for (int innerLoc = 0; innerLoc < currentSection.Word.Length; innerLoc++)
                    {
                        Unit focus = currentSection.Word[innerLoc];
                        string spacing = buildSpacing(loc, focus, currentSection, innerLoc, inQuotes);
                        if (focus.Typed && focus.Correct)
                        {
                            Run typedText = buildRun(focus, spacing, Colors.DarkGray);
                            Display.Inlines.Add(typedText);
                        }
                        else if (focus.Typed && !focus.Correct)
                        {
                            Run incorrectText = buildRun(focus, spacing, Colors.DarkRed);
                            Display.Inlines.Add(incorrectText);
                        }
                        else
                        {
                            Run untypedText = buildRun(focus, spacing, Colors.White);
                            Display.Inlines.Add(untypedText);
                        }

                    }
                }
            }
        }

        string buildSpacing(int loc, Unit focus, Section currentSection, int innerLoc, bool inQuotes)
        {
            string spacing = "";
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

        string buildText(Unit focus, string spacing)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(focus);
            builder.Append(spacing);

            return builder.ToString();
        }

        Run buildRun(Unit focus, string spacing, Color color)
        {
            Run run = new Run();
            run.Text = buildText(focus, spacing);
            run.Foreground = new SolidColorBrush(color);

            return run;
        }

        bool EndOfChapter()
        {
            if (trainer.Place >= trainer.Sections.Length) return true;
            else return false;
        }

    }

    public class Trainer
    {
        public static readonly char START_QUOTATION = (char)8220;
        public static readonly char END_QUOTATION = (char)8221;

        Section[] sections;
        public Section[] Sections { get { return sections; } set { sections = value; } }

        int place;
        public int Place { get { return place; } set { place = value; } }

        int focus;
        public int Focus { get { return focus; } set { focus = value; } }

        int wordsTyped;
        public int WordsTyped { get { return wordsTyped; } set { wordsTyped = value; } }

        public Trainer(string filename)
        {
            wordsTyped = 0;
            place = 0;
            focus = 0;
            sections = ParseData(filename);
        }

        public void Restart()
        {
            wordsTyped = 0;
            focus = 0;
            foreach (Section section in sections)
            {
                section.unTypeAll();
                section.clearAllCorrect();
            }
        }

        Section[] ParseData(string filename)
        {
            string text = "";
            if (filename.Contains(".html"))
            {
                var doc = new HtmlDocument();
                doc.Load(filename);
                var node = doc.DocumentNode.SelectNodes("//p");
                text = "";
                foreach (var n in node)
                {
                    text += " " + n.InnerText;
                }
            }
            else if (filename.Contains(".txt"))
            {
                text = File.ReadAllText(filename);
            }
            else throw new Exception("File type supported");

            text = ConvertContent(text);
            char[] textChars = text.ToCharArray();

            List<Section> words = new List<Section>();
            List<Unit> units = new List<Unit>();
            foreach (char c in textChars)
            {
                if (!Char.IsWhiteSpace(c))
                {
                    units.Add(new Unit(c));
                }
                else
                {
                    words.Add(new Section(units.ToArray()));
                    units = new List<Unit>();
                }
            }
            if (units.Count > 0)
            {
                words.Add(new Section(units.ToArray()));
                units = new List<Unit>();
            }
            words = words.Where(s => !string.IsNullOrWhiteSpace(s.ToString())).ToList();

            return words.ToArray();
        }

        string ConvertContent(string rawText)
        {

            if (rawText.Contains((char)169))
            {
                rawText = rawText.Substring(0, rawText.IndexOf((char)169));
            }
            if (rawText.Contains("Does anyone want to become a moderator for this novel?"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("Does anyone want to become a moderator for this novel?"));
            }
            if (rawText.Contains("More Privileged Chapters Download the app"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("More Privileged Chapters Download the app"));
            }
            if (rawText.Contains("Find authorized novels in Webnovel"))
            {
                string fluff = rawText.Substring(rawText.IndexOf("Find authorized novels in Webnovel"),
                                (rawText.LastIndexOf("a&gt; for visiting.") + 19) - rawText.IndexOf("Find authorized novels in Webnovel"));

                rawText = rawText.Replace(fluff, "");
            }

            rawText = rawText.Replace("&nbsp;", "");
            rawText = rawText.Replace((char)8217, '\'');
            rawText = rawText.Replace((char)171, START_QUOTATION);
            rawText = rawText.Replace((char)187, END_QUOTATION);


            return rawText;
        }

        public bool Rewind(int change)
        {

            if (place == 0) return false;
            bool inQuotes = false;
            for (int focus = place; focus > 0; focus--)
            {
                Section section = sections[focus - 1];
                if (section.ToString().Last().Equals(END_QUOTATION)) inQuotes = true;
                else if (section.ToString().First().Equals(START_QUOTATION)) inQuotes = false;
                if (change <= 0)
                {
                    place = focus + 1;
                    Restart();
                    return true;
                }
                else if (!inQuotes && (section.ToString().Last().Equals('.') || section.ToString().Last().Equals('!') || section.ToString().Last().Equals('?')))
                {
                    change--;
                }
            }
            place = 0;
            return true;

            // Old implementation
            /*if (place - change >= 0)
            {
                place -= change;
                Restart();
                return true;
            }
            else if (place == 0)
            {
                return false;
            }
            else if (place - change < 0)
            {
                place = 0;
                Restart();
                return true;
            }
            else return false;*/
        }

        public bool Forward(int change)
        {

            if (place == sections.Length) return false;
            try
            {
                bool inQuotes = false;
                for (int focus = place; focus <= sections.Length; focus++)
                {
                    Section section = sections[focus + 1];
                    if (section.ToString().First().Equals(START_QUOTATION)) inQuotes = true;
                    else if (section.ToString().Last().Equals(END_QUOTATION)) inQuotes = false;
                    if (change <= 0)
                    {
                        place = focus + 1;
                        Restart();
                        return true;
                    }
                    else if (!inQuotes && (section.ToString().Last().Equals('.') || section.ToString().Last().Equals('!') || section.ToString().Last().Equals('?')))
                    {
                        change--;
                    }
                }
                place = sections.Length;
                return false;

            }
            catch (IndexOutOfRangeException error)
            {
                place = sections.Length;
                return true;
            }

            // Old Implementation
            /*if (place + change <= sections.Length - displaySize)
            {
                place += change;
                Restart();
                return true;
            }
            else if (place == sections.Length - displaySize)
            {
                return false;
            }
            else if (place + change > sections.Length - displaySize)
            {
                place = sections.Length - displaySize;
                Restart();
                return true;
            }
            else return false;*/
        }

        public bool NextWord()
        {
            focus = 0;
            place++;
            wordsTyped++;
            return true;
        }

        public bool Check(char character)
        {
            if (focus >= sections[place].Word.Length)
            {
                focus = sections[place].Word.Length;
                return false;
            }
            Unit currentUnit = sections[place].Word[focus];
            if (character.Equals(currentUnit.Character) || SpecialMatch(character, currentUnit))
            {
                currentUnit.Correct = true;
                currentUnit.Typed = true;
                focus++;
                return true;
            }
            else
            {
                currentUnit.Correct = false;
                currentUnit.Typed = true;
                focus++;
                return false;
            }
        }

        private bool SpecialMatch(char character, Unit currentUnit)
        {
            if (character.Equals('c') && currentUnit.Equals((char)169)) return true;
            if (character.Equals('-') && currentUnit.Equals((char)8211)) return true;
            if (character.Equals('"') && currentUnit.Equals((char)8220)) return true;
            if (character.Equals('"') && currentUnit.Equals((char)8221)) return true;
            if (character.Equals('.') && currentUnit.Equals((char)8230)) return true;
            else return false;
        }

        public bool Space()
        {
            if (focus >= sections[place].Word.Length && WordCorrectlyTyped()) return NextWord();
            else return false;
        }

        private bool WordCorrectlyTyped()
        {
            foreach (Unit unit in sections[place].Word)
            {
                if (!unit.Correct) return false;
            }
            return true;
        }

        public bool BackSpace()
        {
            if (focus == 0)
            {
                sections[place].Word[focus].Typed = false;
                sections[place].Word[focus].Correct = false;
                return false;
            }
            else
            {
                sections[place].Word[--focus].Typed = false;
                sections[place].Word[focus].Correct = false;
                return true;
            } 
        }

    }

    public class Section
    {
        Unit[] word;
        public Unit[] Word { get { return word; } set { word = value; } }

        public Section(Unit[] word)
        {
            Word = word;
        }

        public void unTypeAll()
        {
            foreach (Unit unit in word)
            {
                unit.Typed = false;
            }
        }

        public void clearAllCorrect()
        {
            foreach (Unit unit in word)
            {
                unit.Correct = false;
            }
        }
        public override string ToString()
        {
            string result = "";
            if (word.Length > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Unit unit in word)
                {
                    stringBuilder.Append(unit.Character);
                }
                result = stringBuilder.ToString();
            }

            return result;
        }
    }

    public class Unit
    {
        char character;
        public char Character { get { return character; } set { character = value; } }

        bool typed;
        public bool Typed { get { return typed; } set { typed = value; } }

        bool correct;
        public bool Correct { get { return correct; } set { correct = value; } }

        public Unit(char character)
        {
            Character = character;
        }

        public override bool Equals(object obj)
        {
            return character.Equals(obj);
        }

        public override string ToString()
        {
            return character.ToString();
        }
    }

    public class InputParser
    {
        
        public static bool IsShiftPressed()
        {
            var ShiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
            return (ShiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        public static bool IsAltPressed()
        {
            var altState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);
            return (altState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        public static char GetCharEquivalent(Windows.UI.Core.KeyEventArgs e)
        {

            switch (e.VirtualKey.ToString())
            {
                case "Number1": return IsShiftPressed() ? '!' : '1';
                case "Number2": return IsShiftPressed() ? '@' : '2';
                case "Number3": return IsShiftPressed() ? '#' : '3';
                case "Number4": return IsShiftPressed() ? '$' : '4';
                case "Number5": return IsShiftPressed() ? '%' : '5';
                case "Number6": return IsShiftPressed() ? '^' : '6';
                case "Number7": return IsShiftPressed() ? '&' : '7';
                case "Number8": return IsShiftPressed() ? '*' : '8';
                case "Number9": return IsShiftPressed() ? '(' : '9';
                case "Number0": return IsShiftPressed() ? ')' : '0';
                case "189": return IsShiftPressed() ? '_' : '-';
                case "187": return IsShiftPressed() ? '+' : '=';
                case "Q": return IsShiftPressed() ? 'Q' : 'q';
                case "W": return IsShiftPressed() ? 'W' : 'w';
                case "E": return IsShiftPressed() ? 'E' : 'e';
                case "R": return IsShiftPressed() ? 'R' : 'r';
                case "T": return IsShiftPressed() ? 'T' : 't';
                case "Y": return IsShiftPressed() ? 'Y' : 'y';
                case "U": return IsShiftPressed() ? 'U' : 'u';
                case "I": return IsShiftPressed() ? 'I' : 'i';
                case "O": return IsShiftPressed() ? 'O' : 'o';
                case "P": return IsShiftPressed() ? 'P' : 'p';
                case "A": return IsShiftPressed() ? 'A' : 'a';
                case "S": return IsShiftPressed() ? 'S' : 's';
                case "D": return IsShiftPressed() ? 'D' : 'd';
                case "F": return IsShiftPressed() ? 'F' : 'f';
                case "G": return IsShiftPressed() ? 'G' : 'g';
                case "H": return IsShiftPressed() ? 'H' : 'h';
                case "J": return IsShiftPressed() ? 'J' : 'j';
                case "K": return IsShiftPressed() ? 'K' : 'k';
                case "L": return IsShiftPressed() ? 'L' : 'l';
                case "186": return IsShiftPressed() ? ':' : ';';
                case "222": return IsShiftPressed() ? '"' : '\'';
                case "Z": return IsShiftPressed() ? 'Z' : 'z';
                case "X": return IsShiftPressed() ? 'X' : 'x';
                case "C": return IsShiftPressed() ? 'C' : 'c';
                case "V": return IsShiftPressed() ? 'V' : 'v';
                case "B": return IsShiftPressed() ? 'B' : 'b';
                case "N": return IsShiftPressed() ? 'N' : 'n';
                case "M": return IsShiftPressed() ? 'M' : 'm';
                case "188": return IsShiftPressed() ? '<' : ',';
                case "190": return IsShiftPressed() ? '>' : '.';
                case "191": return IsShiftPressed() ? '?' : '/';
                case "192": return '`';
                default: return '~';
            }
        }
    }
}
