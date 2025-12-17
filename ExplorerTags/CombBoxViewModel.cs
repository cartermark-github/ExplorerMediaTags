using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExplorerTags
{
    public class ComboBoxViewModel : INotifyPropertyChanged
    {
        // 1. The Collection Property (Source for the ComboBox)
        public ObservableCollection<string> AvailableOptions { get; set; }


        // 2. Property to hold the currently selected item (Optional, but often needed)
        private string _selectedOption;
        public string SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged(nameof(SelectedOption));
                    // You can execute logic here based on the new selection
                }
            }
        }

        // Command to demonstrate adding an item at runtime
        //public ICommand AddItemCommand { get; }

        public ComboBoxViewModel()
        {
            // Initialize the collection and populate it with initial data
            AvailableOptions = new ObservableCollection<string>
            {
                "Option A",
                "Option B",
                "Option C"
            };

            //AddItemCommand = new RelayCommand(ExecuteAddItem);
            ExecuteAddItem();
        }

        private void ExecuteAddItem()
        {
            // 3. Adding an item is done by manipulating the collection, NOT the ComboBox control.
            string newItem = $"New Item {AvailableOptions.Count + 1}";
            AvailableOptions.Add(newItem);

            // The ComboBox UI will automatically update because it is bound to an ObservableCollection.
        }


        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
