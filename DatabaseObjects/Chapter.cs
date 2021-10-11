using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer.DatabaseObjects
{
    public class Chapter : INotifyPropertyChanged
    {
        public int ChapterID { get; set; }
        public int ChapterNumber { get; set; }
        public string ChapterText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
