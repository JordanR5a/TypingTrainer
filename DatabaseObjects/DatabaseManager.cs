using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;

namespace TypingTrainer.DatabaseObjects
{
    class DatabaseManager
    {
        static readonly string DATABASE_CONNECTION = @"Data Source=RZ-LAPTOP,1433;Initial Catalog=TT_Data;User ID=sa;Password=HFq1qYSE7LP3vHsRmVBa";

        public static Book GetBook(string novelName)
        {
            string GetBookQuery = String.Format("SELECT TOP 1 * FROM books WHERE Book_title = '{0}'", novelName);

            try
            {
                Book book = null;
                using (SqlConnection conn = new SqlConnection(DATABASE_CONNECTION))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetBookQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    book = new Book();
                                    book.BookID = reader.GetInt32(0);
                                    book.BookTitle = reader.GetString(1);
                                }
                            }
                        }
                    }
                }
                return book;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static ObservableCollection<Chapter> GetChapters(int novel)
        {
            string GetBookQuery = String.Format("SELECT Chapters.ChapterID, Chapters.chapter_number, Chapters.chapter_text "
                                            + "FROM Chapters "
                                            + "INNER JOIN books ON books.bookid = Chapters.book_bookid "
                                            + "WHERE books.bookid = {0}", novel);

            var chapters = new ObservableCollection<Chapter>();
            try
            {
                using (SqlConnection conn = new SqlConnection(DATABASE_CONNECTION))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetBookQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var chapter = new Chapter();
                                    chapter.ChapterID = reader.GetInt32(0);
                                    chapter.ChapterNumber = reader.GetInt32(1);
                                    chapter.ChapterText = reader.GetString(2);
                                    chapters.Add(chapter);
                                }
                            }
                        }
                    }
                }
                return chapters;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }
    }
}
