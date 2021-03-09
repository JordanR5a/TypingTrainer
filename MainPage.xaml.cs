﻿using HtmlAgilityPack;
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
        static readonly int DISPLAY_SIZE = 150;
        static string CURRENT_NOVEL = "Martial_World";

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

            trainer = new Trainer(filename + ".html", DISPLAY_SIZE);

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
            var element = new MediaElement();
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Resources");
            var file = await folder.GetFileAsync(filename);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            element.SetSource(stream, "");
            element.Play();
        }

        async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            

            if (e.VirtualKey == VirtualKey.Escape && !AnyContentDialogOpen())
            {
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
            }
            else if (e.VirtualKey == VirtualKey.CapitalLock && !AnyContentDialogOpen())
            {
                updateWPMDisplay();
            }
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
            else if (e.VirtualKey == VirtualKey.Down && !AnyContentDialogOpen())
            {
                if (trainer.Forward(1))
                {
                    timer.Stop();
                    await PlaySound("return.mp3");
                    startText = trainer.Place;
                    trainer.Restart();
                    updateMainDisplay();
                }
                else await PlaySound("error.wav");
            }
            else if (e.VirtualKey == VirtualKey.PageDown && !AnyContentDialogOpen())
            {
                if (trainer.Forward(5))
                {
                    timer.Stop();
                    await PlaySound("shortMove.wav");
                    startText = trainer.Place;
                    trainer.Restart();
                    updateMainDisplay();
                }
                else await PlaySound("error.wav");
            }
            else if (e.VirtualKey == VirtualKey.Up && !AnyContentDialogOpen())
            {
                if (trainer.Rewind(2))
                {
                    timer.Stop();
                    await PlaySound("return.mp3");
                    startText = trainer.Place;
                    trainer.Restart();
                    updateMainDisplay();
                }
                else await PlaySound("error.wav");
            }
            else if (e.VirtualKey == VirtualKey.PageUp && !AnyContentDialogOpen())
            {
                if (trainer.Rewind(6))
                {
                    timer.Stop();
                    await PlaySound("shortMove.wav");
                    startText = trainer.Place;
                    trainer.Restart();
                    updateMainDisplay();
                }
                else await PlaySound("error.wav");
            }
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

        bool AnyContentDialogOpen()
        {
            var openedpopups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in openedpopups)
            {
                if (popup.Child is ContentDialog) return true;
            }
            return false;
        }

        void updateWPMDisplay()
        {
            if (WPMData.Count > 0)
            {
                timer.Stop();
                WPMDisplay.Opacity = 0.8;
                double average = WPMData.Average();
                WPMDisplay.Text = string.Format("Seconds: {0}\n WPM: {1:F2}", totalSeconds, average);
            }
            else PlaySound("error.wav");
        }

        void updateMainDisplay()
        {
            Display.Inlines.Clear();
            if (EndOfChapter())
            {
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
                Display.FontSize = 34;
                Display.TextAlignment = TextAlignment.Left;

                int dynamicDisplaySize;
                if (startText + DISPLAY_SIZE > trainer.Sections.Length) dynamicDisplaySize = trainer.Sections.Length;
                else dynamicDisplaySize = startText + DISPLAY_SIZE;

                for (int loc = startText; loc < dynamicDisplaySize; loc++)
                {
                    Section currentSection = trainer.Sections[loc];
                    for (int innerLoc = 0; innerLoc < currentSection.Word.Length; innerLoc++)
                    {
                        Unit focus = currentSection.Word[innerLoc];
                        if (focus.Typed && focus.Correct)
                        {
                            StringBuilder typedBuilder = new StringBuilder();
                            typedBuilder.Append(focus);
                            if (innerLoc == currentSection.Word.Length - 1) typedBuilder.Append(" ");
                            typedBuilder.Replace(". ", ".\n\n");
                            typedBuilder.Replace("! ", ".\n\n");
                            typedBuilder.Replace("? ", ".\n\n");
                            typedBuilder.Replace($"{(char)34} ", $"{(char)34}\n\n");

                            Run typedText = new Run();
                            typedText.Text = typedBuilder.ToString();
                            typedText.Foreground = new SolidColorBrush(Colors.DarkGray);

                            Display.Inlines.Add(typedText);
                        }
                        else if (focus.Typed && !focus.Correct)
                        {
                            StringBuilder incorrectBuilder = new StringBuilder();
                            incorrectBuilder.Append(focus);
                            if (innerLoc == currentSection.Word.Length - 1) incorrectBuilder.Append(" ");
                            incorrectBuilder.Replace(". ", ".\n\n");
                            incorrectBuilder.Replace("! ", ".\n\n");
                            incorrectBuilder.Replace("? ", ".\n\n");
                            incorrectBuilder.Replace($"{(char)34} ", $"{(char)34}\n\n");

                            Run incorrectText = new Run();
                            incorrectText.Text = incorrectBuilder.ToString();
                            incorrectText.Foreground = new SolidColorBrush(Colors.DarkRed);

                            Display.Inlines.Add(incorrectText);
                        }
                        else
                        {
                            StringBuilder untypedBuilder = new StringBuilder();
                            untypedBuilder.Append(focus);
                            if (innerLoc == currentSection.Word.Length - 1) untypedBuilder.Append(" ");
                            untypedBuilder.Replace(". ", ".\n\n");
                            untypedBuilder.Replace("! ", ".\n\n");
                            untypedBuilder.Replace("? ", ".\n\n");
                            untypedBuilder.Replace($"{(char)34} ", $"{(char)34}\n\n");

                            Run untypedText = new Run();
                            untypedText.Text = untypedBuilder.ToString();

                            Display.Inlines.Add(untypedText);
                        }

                    }
                }
            }
        }

        bool EndOfChapter()
        {
            if (trainer.Place >= trainer.Sections.Length) return true;
            else return false;
        }

    }

    public class Trainer
    {
        Section[] sections;
        public Section[] Sections { get { return sections; } set { sections = value; } }

        int place;
        public int Place { get { return place; } set { place = value; } }

        int focus;
        public int Focus { get { return focus; } set { focus = value; } }

        int wordsTyped;
        public int WordsTyped { get { return wordsTyped; } set { wordsTyped = value; } }

        int displaySize;

        public Trainer(string filename, int displaySize)
        {
            this.displaySize = displaySize;
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

            text = TrimContent(text);
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

        string TrimContent(string rawText)
        {

            if (rawText.Contains((char)169))
            {
                rawText = rawText.Substring(0, rawText.IndexOf((char)169));
            }
            if (rawText.Contains("Does anyone want to become a moderator for this novel?"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("Does anyone want to become a moderator for this novel?"));
            }
            if (rawText.Contains("&nbsp;"))
            {
                rawText = rawText.Replace("&nbsp;", "");
            }

            return rawText;
        }

        public bool Rewind(int change)
        {

            if (place == 0) return false;
            for (int focus = place; focus > 0; focus--)
            {
                Section section = sections[focus - 1];
                if (change <= 0)
                {
                    place = focus + 1;
                    Restart();
                    return true;
                }
                else if (section.ToString().Contains(".") || section.ToString().Contains("!") || section.ToString().Contains("?"))
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
                for (int focus = place; focus <= sections.Length; focus++)
                {
                    Section section = sections[focus + 1];
                    if (change <= 0)
                    {
                        place = focus + 1;
                        Restart();
                        return true;
                    }
                    else if (section.ToString().Contains(".") || section.ToString().Contains("!") || section.ToString().Contains("?"))
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
            if (focus >= sections[place].Word.Length) return false;
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
            if (character.Equals('\'') && currentUnit.Equals((char)8217)) return true;
            if (character.Equals('-') && currentUnit.Equals((char)8211)) return true;
            if (character.Equals('.') && currentUnit.Equals((char)8230)) return true;
            if (character.Equals('"') && currentUnit.Equals((char)8221)) return true;
            if (character.Equals('"') && currentUnit.Equals((char)8220)) return true;
            if (character.Equals('c') && currentUnit.Equals((char)169)) return true;
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
