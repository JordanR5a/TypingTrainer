using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.ApplicationObjects
{
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
}
