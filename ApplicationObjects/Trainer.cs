using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TypingTrainer.DatabaseObjects;

namespace TypingTrainer.ApplicationObjects
{
    public class Trainer
    {
        public static readonly char START_QUOTATION = (char)8220;
        public static readonly char END_QUOTATION = (char)8221;

        Book novel;
        public Book Novel { get { return novel; } }

        int currentChapterNumber;
        public int CurrentChapterNumber { get { return currentChapterNumber; } }

        List<Chapter> chapters;
        public List<Chapter> Chapters { get { return chapters; } }

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
            currentChapterNumber = GetFirstNovel(novelName);
            wordsTyped = 0;
            place = 0;
            focus = 0;
            sections = ParseData();
        }

        int GetFirstNovel(string novelName)
        {
            novel = DatabaseManager.GetBook(novelName);
            if (novel == null) throw new DirectoryNotFoundException();
            chapters = DatabaseManager.GetChapters(novel.BookID);
            Chapter minNumChapter = chapters.First();
            foreach (var chapter in chapters)
            {
                if (chapter.ChapterNumber < minNumChapter.ChapterNumber)
                    minNumChapter = chapter;
            }
            return minNumChapter.ChapterNumber;
        }

        public bool NextChapter()
        {
            int nextChapter = currentChapterNumber + 1;
            foreach (var chapter in chapters)
            {
                if (chapter.ChapterNumber == nextChapter)
                {
                    currentChapterNumber = nextChapter;
                    ReParse();
                    return true;
                }
            }
            return false;
        }
        public bool PreviousChapter()
        {
            int prevChapter = currentChapterNumber - 1;
            foreach (var chapter in chapters)
            {
                if (chapter.ChapterNumber == prevChapter)
                {
                    currentChapterNumber = prevChapter;
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
            sections = ParseData();
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

        Section[] ParseData()
        {
            string text = chapters.Where(x => x.ChapterNumber == currentChapterNumber).FirstOrDefault().ChapterText;
            text = ConvertContent(text);
            char[] textChars = text.ToCharArray();

            List<Section> words = new List<Section>();
            List<Unit> units = new List<Unit>();
            foreach (char c in textChars)
            {
                if (!Char.IsWhiteSpace(c)) units.Add(new Unit(c));
                else
                {
                    Section section = new Section(units.ToArray(), BuildModifier(c));
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
            if (c.Equals('\n') || c.Equals('\r')) return "\n\n";
            else return " ";
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
            if (rawText.Contains("More Privileged Chapters"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("More Privileged Chapters"));
            }
            if (rawText.Contains("Follow me:"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("Follow me:"));
            }
            if (rawText.Contains("Advertisement Pornographic Personal attack"))
            {
                rawText = rawText.Substring(0, rawText.IndexOf("Advertisement Pornographic Personal attack"));
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
                    try
                    {
                        if (section.ToString().Contains(END_QUOTATION)) inQuotes = true;
                        else if (section.ToString().Contains(START_QUOTATION) && sections[focus - 2].ToString().Contains(END_QUOTATION))
                        {
                            inQuotes = true;
                            focus--;
                            change--;
                        }
                        else if (section.ToString().Contains(START_QUOTATION)) inQuotes = false;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        inQuotes = false;
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
                        section = sections[focus];
                        if (section.ToString().Contains(START_QUOTATION)) inQuotes = true;
                        else if (section.ToString().Contains(END_QUOTATION) && sections[focus + 1].ToString().Contains(START_QUOTATION))
                        {
                            inQuotes = false;
                            focus++;
                            change--;
                        }
                        else if (section.ToString().Contains(END_QUOTATION)) inQuotes = false;
                        if (change <= 0)
                        {
                            place = focus;
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
            if (character.Equals('i') && currentUnit.Equals((char)239)) return true;
            if (character.Equals('A') && currentUnit.Equals((char)1040)) return true;
            if (character.Equals('K') && currentUnit.Equals((char)1050)) return true;
            if (character.Equals('y') && currentUnit.Equals((char)1091)) return true;
            if (character.Equals('-') && currentUnit.Equals((char)8211)) return true;
            if (character.Equals('-') && currentUnit.Equals((char)8212)) return true;
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
}
