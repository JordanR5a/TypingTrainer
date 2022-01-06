using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.DatabaseObjects
{
    public class LocalBook : Book
    {
        public BookType Type{ get; set; }

        public LocalBook(int bookID, string bookTitle, BookType type)
        {
            BookID = bookID;
            BookTitle = bookTitle;
            Type = type;
        }

        public enum BookType
        {
            LOCAL,
            PSUEDO_LOCAL,
            EXTERNAL
        }

    }
}
