using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace TypingTrainer.DatabaseObjects
{
    public class ApiManager
    {
        private static async Task<string> TryPostJsonAsync(string url, Object content)
        {
            string httpResponseBody;
            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(url);

                // Construct the JSON to post.
                HttpStringContent httpContent = new HttpStringContent(JsonConvert.SerializeObject(content));

                // Post the JSON and wait for a response.
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(uri, httpContent);

                // Make sure the post succeeded, and write out the response.
                httpResponseMessage.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                return httpResponseBody;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private class PostObject
        {
            public Book book { get; set; }
            public List<Chapter> chapters { get; set; }

            public PostObject(Book book, List<Chapter> chapters)
            {
                this.book = book;
                this.chapters = chapters;
            }
        }

        public static bool CreateBook(Book book, List<Chapter> chapters)
        {
            return TryPostJsonAsync("http://localhost:8080", new PostObject(book, chapters)).GetAwaiter().GetResult().Equals("True");
        }

        private static string SendGetRequest(string url)
        {
            //Create an HTTP client object
            HttpClient httpClient = new HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri(url);

            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = httpClient.GetAsync(requestUri).AsTask().GetAwaiter().GetResult();
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = httpResponse.Content.ReadAsStringAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            return httpResponseBody;
        }

        public static bool SystemOnline()
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            var headers = httpClient.DefaultRequestHeaders;

            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://localhost:8080/");

            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();

            try
            {
                httpResponse = httpClient.GetAsync(requestUri).AsTask().GetAwaiter().GetResult();
                httpResponse.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message);
                return false;
            }
        }

        public static List<Chapter> GetChaptersByBookId(int bookId)
        {
            var relevantChapters = SendGetRequest("http://localhost:8080/chapters/" + bookId);
            try
            {
                return JsonConvert.DeserializeObject<List<Chapter>>(relevantChapters);
            }
            catch (JsonReaderException er) { return new List<Chapter>(); }
        }

        public static List<Book> GetAllBooks()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<Book>>(SendGetRequest("http://localhost:8080"));
            }
            catch (JsonReaderException er) { return new List<Book>(); }
        }

        public static bool BookExists(string bookId)
        {
            try
            {
                return JsonConvert.DeserializeObject<Book>(SendGetRequest("http://localhost:8080/book/" + bookId)) != null;
            }
            catch (JsonReaderException er) { return false; }
        }

        public static bool ChapterExists(string chapterId)
        {
            try
            {
                return JsonConvert.DeserializeObject<Chapter>(SendGetRequest("http://localhost:8080/chapter/" + chapterId)) != null;
            }
            catch (JsonReaderException er) { return false; }
        }
    }
}
