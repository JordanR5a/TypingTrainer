using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace TypingTrainer.ApplicationObjects
{
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
                case "219": return IsShiftPressed() ? '{' : '[';
                case "221": return IsShiftPressed() ? '}' : ']';
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
                case "192": return IsShiftPressed() ? '~' : '`';
                default: return ' ';
            }
        }
    }
}
