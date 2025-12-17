using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerTags
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _taskDescription;
        private bool _isCompleted;

        public string TaskDescription
        {
            get { return _taskDescription; }
            set
            {
                if (_taskDescription != value)
                {
                    _taskDescription = value;
                    OnPropertyChanged(nameof(TaskDescription));
                }
            }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
