using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.DatabaseObjects
{
    class DatabaseManager
    {
        static readonly string DATABASE_CONNECTION = @"Data Source=RZ-LAPTOP;Initial Catalog=TT_Data;User ID=sa;Password=A9WEmkvJBBB55yzcMFry!r$SAt9sn7p2arVfmGxgDBukV6PUrYaPwn%pRB@3";

        public static Book GetBook(string novelName)
        {
            string GetBookQuery = String.Format("SELECT TOP 1 * FROM Books WHERE BookTitle = '{0}'", novelName);

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
            string GetBookQuery = String.Format("SELECT Chapters.ChapterID, Chapters.ChapterNumber, Chapters.ChapterText "
                                            + "FROM Chapters "
                                            + "INNER JOIN BooksChapters ON BooksChapters.ChapterID = Chapters.ChapterID "
                                            + "WHERE BooksChapters.BookID = {0}", novel);

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
