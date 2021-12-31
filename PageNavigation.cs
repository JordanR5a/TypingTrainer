using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TypingTrainer
{
    public class PageNavigation
    {
        private static string Page_Type = "page";
        private static string Page_Args = "args";
        public Stack<Dictionary<string, Object>> Stack { get; set; }
        private Dictionary<string, Object> currentPage;
        private Type mainPage;

        public PageNavigation(Type mainPage)
        {
            this.mainPage = mainPage;
            Stack = new Stack<Dictionary<string, Object>>();
            currentPage = new Dictionary<string, object>
            {
                { Page_Type, mainPage},
                { Page_Args, null }
            };
        }

        public void Navigate(Frame frame, Type type)
        {
            Stack.Push(currentPage);
            currentPage = new Dictionary<string, object>
            {
                { Page_Type, type },
                { Page_Args, null }
            };
            frame.Navigate(type);
        }

        public void Navigate(Frame frame, Type type, Object args)
        {
            Stack.Push(currentPage);
            currentPage = new Dictionary<string, object>
            {
                { Page_Type, type },
                { Page_Args, args }
            };
            frame.Navigate(type, args);
        }

        public void Back(Frame frame)
        {
            currentPage = Stack.Pop();
            if (currentPage[Page_Args] != null) frame.Navigate(currentPage[Page_Type] as Type, currentPage[Page_Args]);
            else frame.Navigate(currentPage[Page_Type] as Type);
        }

        public void Reset(Frame frame)
        {
            Navigate(frame, mainPage);
            Stack = new Stack<Dictionary<string, Object>>();
            currentPage = new Dictionary<string, object>
            {
                { Page_Type, mainPage},
                { Page_Args, null }
            };
        }
    }
}
