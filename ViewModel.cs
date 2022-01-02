using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTrainer
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _inputString;

        public string InputString
        {
            get { return _inputString; }
            set
            {
                _inputString = value;
                RaisePropertyChanged("InputString");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
