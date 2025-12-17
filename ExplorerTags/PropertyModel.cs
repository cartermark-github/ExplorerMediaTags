using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ExplorerTags
{
    public class PropertyModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TodoItem> TodoList { get; set; }

        // The ICollectionView for filtering
        public ICollectionView TodoListView { get; set; }

        private BitmapSource _videoThumbnail;
        private String _VideoFileName;

        public BitmapSource VideoThumbnail
        {
            get { return _videoThumbnail; }
            set
            {
                if (_videoThumbnail != value)
                {
                    _videoThumbnail = value;
                    OnPropertyChanged(nameof(VideoThumbnail));
                }
            }
        }

        public String VideoFileName
        {
            get { return _VideoFileName; }
            set
            {
                if (_VideoFileName != value)
                {
                    _VideoFileName = value;
                    OnPropertyChanged(nameof(VideoFileName));
                }
            }
        }

        // Define the file path for data persistence
        public readonly string _dataFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "WPFChecklist",
                         "tasks.json");


        // Commands bound to buttons
        public ICommand AddCommand { get; set; }





        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
