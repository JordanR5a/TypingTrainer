using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.DatabaseObjects
{
    public class LocalBook : Book
    {
        public bool IsLocal { get; set; }

        public LocalBook(int bookID, string bookTitle, bool isLocal)
        {
            BookID = bookID;
            BookTitle = bookTitle;
            IsLocal = isLocal;
        }

    }
}
