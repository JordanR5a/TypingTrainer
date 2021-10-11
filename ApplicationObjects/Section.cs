using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.ApplicationObjects
{
    public class Section
    {
        Unit[] word;
        public Unit[] Word { get { return word; } set { word = value; } }

        string modifier;
        public string Modifier { get { return modifier; } set { modifier = value; } }

        public Section(Unit[] word, string mod)
        {
            Word = word;
            Modifier = mod;
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
}
