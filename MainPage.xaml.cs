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
        static string CURRENT_NOVEL = "Martial_World";

        int startText;
        bool rawFormat;
        Trainer trainer;

        DispatcherTimer timer;
        List<int> WPMData;
        int totalSeconds;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
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

            trainer = new Trainer(novelName);

            startText = trainer.Place;
            UpdateMainDisplay();
        }

        private void TimerTick(object o, Object e)
        {
            totalSeconds++;
            if (totalSeconds % 60 == 0)
            {
                WPMData.Add(trainer.WordsTyped);
                trainer.WordsTyped = 0;
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
            else if (e.VirtualKey == VirtualKey.CapitalLock && !AnyContentDialogOpen()) UpdateWPMDisplay();
            else if (e.VirtualKey == VirtualKey.Tab && !AnyContentDialogOpen()) rawFormat = rawFormat ? false : true;
            else if (e.VirtualKey == VirtualKey.Right && !AnyContentDialogOpen())
            {
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
                if (trainer.Space())
                {
                    await PlaySound("returnBell.wav", false);
                    startText = trainer.Place;
                }
                else await PlaySound("error.wav", true);
            }
            else if (e.VirtualKey == VirtualKey.Space && !EndOfChapter() && !AnyContentDialogOpen())
            {
                if (trainer.Space()) await PlaySound("space.mp3", false);
                else await PlaySound("error.wav", true);
            }
            else if (e.VirtualKey == VirtualKey.Back && !EndOfChapter() && !AnyContentDialogOpen())
            {
                if (trainer.BackSpace()) await PlaySound("backspace.mp3", false);
                else await PlaySound("error.wav", true);
            }
            else if (InputParser.GetCharEquivalent(e) != '~' && !EndOfChapter() && !AnyContentDialogOpen())
            {
                if (!timer.IsEnabled) timer.Start();
                WPMDisplay.Opacity = 0;
                if (trainer.Check(InputParser.GetCharEquivalent(e))) await PlaySound("erikaTap.mp3", false);
                else await PlaySound("error.wav", true);
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
                    StartChapter(contentDialog.title);
                    CURRENT_NOVEL = contentDialog.title;
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

        async void UpdateWPMDisplay()
        {
            if (WPMData.Count > 0)
            {
                timer.Stop();
                WPMDisplay.Opacity = 0.8;
                double average = WPMData.Average();
                WPMDisplay.Text = $"Time: {string.Format("{0:F2}", totalSeconds / 60.0)}\nWPM: {string.Format("{0:F2}", average)}";
            }
            else await PlaySound("error.wav", true);
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
                CurrentChapterDisplay.Text = "Current Chapter: " + trainer.CurrentChapterNumber;
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
                        string spacing = BuildSpacing(loc, focus, currentSection, innerLoc, inQuotes);
                        if (focus.Typed && focus.Correct)
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

        string BuildSpacing(int loc, Unit focus, Section currentSection, int innerLoc, bool inQuotes)
        {
            if (rawFormat) return "";

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

    public class Trainer
    {
        public static readonly string DATA_FOLDER = "Data";
        public static readonly string DEFAULT_EXTENSION = "txt";
        public static readonly char START_QUOTATION = (char)8220;
        public static readonly char END_QUOTATION = (char)8221;

        string currentNovel;
        public string CurrentNovel { get { return currentNovel; } }

        string currentChapter;
        public string CurrentChapter { get { return currentChapter; } }

        public int CurrentChapterNumber { get { return int.Parse(currentChapter.Split('.')[0]); } }

        Section[] sections;
        public Section[] Sections { get { return sections; } }

        int place;
        public int Place { get { return place; } }

        int focus;
        public int Focus { get { return focus; } }

        int wordsTyped;
        public int WordsTyped { get { return wordsTyped; } set { wordsTyped = value; } }

        public Trainer(string novelName)
        {
            currentChapter = GetFirstNovel(novelName);
            currentNovel = novelName;
            wordsTyped = 0;
            place = 0;
            focus = 0;
            sections = ParseData(GetCurrentPath());
        }

        string GetCurrentPath()
        {
            return DATA_FOLDER + "\\" + currentNovel + "\\" + currentChapter;
        }

        string GetFirstNovel(string novelName)
        {
            string[] availableChapters = Directory.GetFiles(DATA_FOLDER + "\\" + novelName);
            if (availableChapters.Length <= 0) throw new Exception("No avialable chapters");

            string extension = DEFAULT_EXTENSION;
            int firstChapter = int.MaxValue;
            foreach (string chapter in availableChapters)
            {
                string chapterName = chapter.Substring(chapter.IndexOf(novelName) + novelName.Length + 1);
                int chapterNumber;
                string[] parts = chapterName.Split('.');
                if (int.TryParse(parts[0], out chapterNumber))
                {
                    if (chapterNumber < firstChapter)
                    {
                        firstChapter = chapterNumber;
                        extension = parts[1];
                    }
                }
            }
            return firstChapter.ToString() + "." + extension;
        }

        public bool NextChapter()
        {
            string[] availableChapters = Directory.GetFiles(DATA_FOLDER + "\\" + currentNovel);
            string[] parts = currentChapter.Split('.');
            int currentChapterNumber = int.Parse(parts[0]);
            string nextChapter = ++currentChapterNumber + "." + parts[1];
            foreach(string chapter in availableChapters)
            {
                if (chapter.Contains(nextChapter))
                {
                    currentChapter = nextChapter;
                    ReParse();
                    return true;
                }
            }
            return false;
        }
        public bool PreviousChapter()
        {
            string[] availableChapters = Directory.GetFiles(DATA_FOLDER + "\\" + currentNovel);
            string[] parts = currentChapter.Split('.');
            int currentChapterNumber = int.Parse(parts[0]);
            string prevChapter = --currentChapterNumber + "." + parts[1];
            foreach (string chapter in availableChapters)
            {
                if (chapter.Contains(prevChapter))
                {
                    currentChapter = prevChapter;
                    ReParse();
                    return true;
                }
            }
            return false;
        }

        void ReParse()
        {
            wordsTyped = 0;
            place = 0;
            focus = 0;
            sections = ParseData(GetCurrentPath());
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
            if (filename.Contains(".html")) text = GetRawHtml(filename);
            else if (filename.Contains(".txt")) text = File.ReadAllText(filename);
            else throw new Exception("File type supported");

            text = ConvertContent(text);
            char[] textChars = text.ToCharArray();

            List<Section> words = new List<Section>();
            List<Unit> units = new List<Unit>();
            foreach (char c in textChars)
            {
                if (!Char.IsWhiteSpace(c)) units.Add(new Unit(c));
                else
                {
                    words.Add(new Section(units.ToArray(), BuildModifier(c)));
                    units = new List<Unit>();
                }
            }
            if (units.Count > 0)
            {
                words.Add(new Section(units.ToArray(), ""));
                units = new List<Unit>();
            }
            words = words.Where(s => !string.IsNullOrWhiteSpace(s.ToString())).ToList();

            return words.ToArray();
        }

        string BuildModifier(char c)
        {
            string mod = " ";
            if (c.Equals("\n") || c.Equals((char)10)) mod = "\n\n";
            else if (c.Equals("\t")) mod = "\t";
            return mod;
        }

        string GetRawHtml(string filename)
        {
            var doc = new HtmlDocument();
            doc.Load(filename);
            var node = doc.DocumentNode.SelectNodes("//p");
            string text = "";
            foreach (var n in node)
            {
                text += "\n" + n.InnerText;
            }
            return text;
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

        public bool Rewind(int change, bool rawFormat)
        {

            if (place == 0) return false;
            bool inQuotes = false;
            for (int focus = place; focus > 0; focus--)
            {
                Section section = sections[focus - 1];

                if (rawFormat)
                {
                    if (change <= 0)
                    {
                        place = focus + 1;
                        Restart();
                        return true;
                    }
                    else if (section.Modifier.Equals("\n\n"))
                    {
                        change--;
                    }
                }
                else
                {
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

        public bool Forward(int change, bool rawFormat)
        {

            if (place == sections.Length) return false;
            try
            {
                bool inQuotes = false;
                for (int focus = place; focus <= sections.Length; focus++)
                {
                    Section section;

                    if (rawFormat)
                    {
                        section = sections[focus];
                        if (change <= 0)
                        {
                            place = focus;
                            Restart();
                            return true;
                        }
                        else if (section.Modifier.Equals("\n\n"))
                        {
                            change--;
                        }
                    }
                    else
                    {
                        section = sections[focus + 1];
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
            if (character.Equals('\'') && currentUnit.Equals((char)8216)) return true;
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

        string modifier;
        public string Modifier { get { return modifier; } set { modifier = value; } }

        public Section(Unit[] word, string modifier)
        {
            Word = word;
            Modifier = modifier;
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
