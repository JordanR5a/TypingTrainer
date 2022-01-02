using System;
using System.Collections.Generic;
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

        public static List<Book> GetBooks()
        {
            string GetBookQuery = String.Format("SELECT * FROM books");

            try
            {
                List<Book> books = new List<Book>();
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
                                    Book book = new Book();
                                    book.BookID = reader.GetInt32(0);
                                    book.BookTitle = reader.GetString(1);
                                    books.Add(book);
                                }
                            }
                        }
                    }
                }
                return books;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static bool BookExists(string bookId)
        {
            string GetBookQuery = String.Format("SELECT TOP 1 * FROM books WHERE bookid = {0}", bookId);

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
                return book != null;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return false;
        }

        public static bool ChapterExists(string chapterId)
        {
            string GetBookQuery = String.Format("SELECT TOP 1 * FROM chapters WHERE chapterid = {0}", chapterId);

            try
            {
                Chapter chapter = null;
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
                                    chapter = new Chapter();
                                    chapter.ChapterID = reader.GetInt32(0);
                                    chapter.ChapterNumber = reader.GetInt32(1);
                                    chapter.ChapterText = reader.GetString(2);
                                }
                            }
                        }
                    }
                }
                return chapter != null;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return false;
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

        public static bool CreateBook(Book book)
        {
            string bookCreationQuery = String.Format("INSERT INTO books VALUES ({0}, '{1}')",
                                                        book.BookID, book.BookTitle.Replace("'", "''"));

            try
            {
                using (SqlConnection conn = new SqlConnection(DATABASE_CONNECTION))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = bookCreationQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return false;
        }

        public static bool CreateChapter(Chapter chapter, string bookId)
        {
            string chapterCreationQuery = String.Format(@"INSERT INTO chapters VALUES ({0}, {1}, '{2}', {3})",
                                                        chapter.ChapterID, chapter.ChapterNumber, chapter.ChapterText.Replace("'", "''"), bookId);

            try
            {
                using (SqlConnection conn = new SqlConnection(DATABASE_CONNECTION))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = chapterCreationQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return false;
        }
    }
}
