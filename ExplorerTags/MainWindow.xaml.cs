using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ExplorerTags
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TodoItem> TodoList { get; set; }

        public ObservableCollection<string> AvailableOptions { get; set; }

        // The ICollectionView for filtering
        public ICollectionView TodoListView { get; set; }

        private BitmapSource _videoThumbnail;
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

        

        private Visibility _statusVisibility = Visibility.Visible;
        public Visibility StatusVisibility
        {
            get { return _statusVisibility; }
            set
            {
                if (_statusVisibility != value)
                {
                    _statusVisibility = value;
                    OnPropertyChanged(nameof(StatusVisibility));
                }
            }
        }

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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                // Trigger the main filter update whenever the search text changes
                ApplyFilters();
                (ClearSearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AddFilterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }


        private String _VideoFileName;
        public String VideoFileName
        {
            get { return _VideoFileName; }
            set
            {
                if (_VideoFileName != value)
                {
                    _VideoFileName = value;
                    OnPropertyChanged(nameof(VideoFileName));
                    (UpdateVideoTagsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (OpenMediaCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private TodoItem _selectedItem;
        public TodoItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                // Force re-evaluation of CanExecute for the DeleteCommand
                (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _checkedItemsString;
        public string CheckedItemsString
        {
            get => _checkedItemsString;
            set
            {
                _checkedItemsString = value;
                OnPropertyChanged(nameof(CheckedItemsString));
                (UpdateVideoTagsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CopyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UncheckAllCommand as RelayCommand)?.RaiseCanExecuteChanged();
                
            }
        }

        // Define the file path for data persistence
        public readonly string _dataFilePath =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "WPFChecklist",
                         "tasks.json");

        public readonly string _filterFilePath =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "WPFChecklist",
                         "filters.json");



        // Commands bound to buttons
        public ICommand SaveCommand { get; private set; }
        public ICommand UncheckAllCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand UpdateVideoTagsCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand OpenMediaCommand { get; private set; }
        public ICommand ClearSearchCommand { get; private set; }
        public ICommand AddFilterCommand { get; private set; }






        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;

            TodoList = LoadTasks();
            AvailableOptions = LoadFilters();

            //AvailableOptions = new ObservableCollection<string>();
            

            CreateCheckedItemString();

            // Subscribe to PropertyChanged for all initially loaded items
            foreach (var item in TodoList)
            {
                item.PropertyChanged += TodoItem_PropertyChanged;
                
            }

            // Subscribe to the collection's changes (for items added/removed)
            TodoList.CollectionChanged += TodoList_CollectionChanged;

            TodoListView = CollectionViewSource.GetDefaultView(TodoList);
            TodoListView.Filter = null; // Default: show all
            ApplySorting();

            // Initialize Commands
            UncheckAllCommand = new RelayCommand(ExecuteUncheckAllCommand, CanExecuteUncheckAllCommand);
            AddCommand = new RelayCommand(ExecuteAddCommand);
            DeleteCommand = new RelayCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            UpdateVideoTagsCommand = new RelayCommand(ExecuteUpdateVideoTagsCommand, CanExecuteUpdateVideoTagsCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            OpenMediaCommand = new RelayCommand(ExecuteOpenMediaCommand, CanExecuteOpenMediaCommand);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearchCommand, CanExecuteClearSearchCommand);
            AddFilterCommand = new RelayCommand(ExecuteAddfilterCommand, CanExecuteAddFilterCommand);
        }   

        public void ApplySorting()
        {
            TodoListView.SortDescriptions.Clear();

            TodoListView.SortDescriptions.Add(
                new SortDescription(nameof(TodoItem.TaskDescription), ListSortDirection.Ascending));
        }

        /// <summary>
        /// Applies the SearchText filter.
        /// </summary>
        private void ApplyFilters()
        {
            if (TodoListView == null) return;

            TodoListView.Filter = item =>
            {
                TodoItem todoItem = item as TodoItem;
                if (todoItem == null) return false;

                bool searchMatch = true;
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    // Check if the task description contains the search text (case-insensitive)
                    searchMatch = todoItem.TaskDescription.IndexOf(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase) >= 0;
                }

                return  searchMatch;
            };

            TodoListView.Refresh();
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (SaveCommand.CanExecute(null))
            {
                SaveCommand.Execute(null);
            }
            SaveFilters();
        }

        /// <summary>
        /// Handles changes to the TodoList collection (adddiding or removing items).
        /// </summary>
        private void TodoList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Subscribe to newly added items
            if (e.NewItems != null)
            {
                foreach (TodoItem item in e.NewItems)
                {
                    item.PropertyChanged += TodoItem_PropertyChanged;
                }
            }

            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (TodoItem item in e.OldItems)
                {
                    item.PropertyChanged -= TodoItem_PropertyChanged;
                }
            }
            CreateCheckedItemString();
        }

        /// <summary>
        /// Handles PropertyChanged event on individual TodoItem objects.
        /// </summary>
        private void TodoItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We only care about changes to the IsCompleted property
            if (e.PropertyName == nameof(TodoItem.IsCompleted))
            {
                HandleItemStatusChange(sender as TodoItem);
            }

            // You might want to refresh the view if the filter is active and the status change
            // would change the visibility of the item.
            TodoListView.Refresh();
        }

        

        /// <summary>
        /// 🔥 THIS IS YOUR PROCEDURE! Run this every time an item is checked/unchecked.
        /// </summary>
        private void HandleItemStatusChange(TodoItem changedItem)
        {
            CreateCheckedItemString();
        }

        private void CreateCheckedItemString()
        {
            string result = string.Join(";",
                TodoList
                    .Where(item => item.IsCompleted)
                    .Select(item => item.TaskDescription)
            );
            
            CheckedItemsString = result;
        }

        private bool CanExecuteClearSearchCommand(object parameter) => !string.IsNullOrEmpty(SearchText);
        private void ExecuteClearSearchCommand(object parameter)
        {
            SearchText = string.Empty;
        }

        private bool CanExecuteAddFilterCommand(object parameter) => !string.IsNullOrEmpty(SearchText);
        private void ExecuteAddfilterCommand(object parameter)
        {
            string itemToAdd = SearchText?.Trim();

            bool itemExists = AvailableOptions.Any(option =>
                option.Equals(itemToAdd, StringComparison.OrdinalIgnoreCase)
            );

            if (itemExists)
            {
                DisplayMessage("⚠ Item already exists in the filter list.");
            }
            else
            {
                AvailableOptions.Add(itemToAdd);
                DisplayMessage("✓ Item added to the filter list.");
            }  
        }

        private bool CanExecuteOpenMediaCommand(object parameter) => 
            !string.IsNullOrEmpty(VideoFileName);
        private void ExecuteOpenMediaCommand(object parameter)
        {
            try
            {
                var processInfo = new ProcessStartInfo(VideoFileName)
                {
                    // UseShellExecute is essential for Windows to use the file's default application.
                    // It is automatically true for .NET Core on non-Windows systems but setting it is good practice.
                    UseShellExecute = true
                };

                // Start the process
                Process.Start(processInfo);
                DisplayMessage("▶ Media opened successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Opening File: {ex.Message}");
            }
        }

        private async void DisplayMessage(string message, int durationMilliseconds = 3000)
        {
            StatusVisibility = Visibility.Hidden;
            SuccessMessage = message;
            await Task.Delay(durationMilliseconds);
            SuccessMessage = string.Empty;
            StatusVisibility = Visibility.Visible;
        }   

        private bool CanExecuteUpdateVideoTagsCommand(object parameter) =>
            !string.IsNullOrEmpty(CheckedItemsString) &&
            !string.IsNullOrEmpty(VideoFileName);
        private void ExecuteUpdateVideoTagsCommand(object parameter)
        {
            string[] NewTags = CheckedItemsString.Split(
                new char[] {';'},
                StringSplitOptions.RemoveEmptyEntries );

            try
            {
                using (ShellFile shellFile = ShellFile.FromFilePath(VideoFileName))
                {
                    shellFile.Properties.System.Keywords.AllowSetTruncatedValue = true;
                    shellFile.Properties.System.Keywords.Value = NewTags;
                    DisplayMessage("✓ Tags updated successfully.");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Error Updating Tags: {ex.Message}\n\n" +
                    $"Note: Close Media Before Updating Tags.");
            }
        }

        private bool CanExecuteUncheckAllCommand(object parameter) => !string.IsNullOrEmpty(CheckedItemsString);
        private void ExecuteUncheckAllCommand(object parameter)
        {   
                foreach (var item in TodoList)
                {
                    item.IsCompleted = false;
                }
        }

        private bool CanExecuteCopyCommand(object parameter) => !string.IsNullOrEmpty(CheckedItemsString);
        private void ExecuteCopyCommand(object parameter)
        {
            try
            {
                StatusVisibility = Visibility.Hidden;
                // Use the WPF Clipboard class to set the text
                Clipboard.SetText(CheckedItemsString);

                // Show the success message
                DisplayMessage("📋 Checked items list copied!");
            }
            catch (Exception ex)
            {
                // Show error message instead
                MessageBox.Show($"Failed to copy: {ex.Message}");
                // Optionally set a timer to clear this error message too                
            }
        }

        private bool CanExecuteDeleteCommand(object parameter) => SelectedItem != null;
        private void ExecuteDeleteCommand(object parameter)
        {
            // IMPORTANT: Must remove from the source collection (TodoList), not the view.
            TodoList.Remove(SelectedItem);
            // ObservableCollection updates the view automatically on removal.
        }

        private void ExecuteAddCommand(object parameter)
        {
            TodoList.Add(new TodoItem { TaskDescription = "New-Tag", IsCompleted = false });
            TodoListView.Refresh();
        }

        private void LoadThumbnail()
        {
            //string videoFile = @"C:\Users\hawny\Videos\Batman.mp4";
            string videoFile =  VideoFileName;

            // 1. Get the System.Drawing.Bitmap (using the Windows API Code Pack method)
            System.Drawing.Bitmap drawingBitmap = APICodePackHelpers.GetVideoThumbnail(videoFile); // Assume this method exists

            if (drawingBitmap != null)
            {
                // 2. Convert System.Drawing.Bitmap to WPF BitmapSource
                VideoThumbnail = ImageConversion.ToBitmapSource(drawingBitmap);

                // NOTE: It is important to dispose of the original System.Drawing.Bitmap
                drawingBitmap.Dispose();
            }
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            // Check if the dragged item contains file paths
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Set the effect to Copy, indicating that files can be dropped
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                // Otherwise, forbid the drop operation
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true; // Mark the event as handled
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            // Get the file paths as a string array
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                VideoFileName = files[0];
                LoadThumbnail();
                using (ShellFile shellFile = ShellFile.FromFilePath(VideoFileName))
                {
                    IEnumerable<string> existingTags = shellFile.Properties.System.Keywords.Value ?? new string[0];
                    if (existingTags.Count()  > 0)
                    {
                        //if (UncheckAllCommand.CanExecute(null))
                        //{
                        //    UncheckAllCommand.Execute(null);
                        //}

                        //Check existing tags from video.
                        var tagsToUpdate = TodoList.Where(ToDoTag =>
                            existingTags.Any(vidTag =>
                            ToDoTag.TaskDescription.IndexOf(vidTag, StringComparison.OrdinalIgnoreCase) >= 0)
                        ).ToList();

                        foreach (TodoItem item in tagsToUpdate)
                        {
                            item.IsCompleted = true;
                        }

                        //Add new tags from video
                        var matchingExistingTags = existingTags.Where(t =>
                            !TodoList.Any(y =>
                                y.TaskDescription.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                            )
                        ).ToList();

                        foreach (var tag in matchingExistingTags)
                        {
                            TodoList.Add(new TodoItem { TaskDescription = tag, IsCompleted = true });
                            TodoListView.Refresh();
                        }

                    }
                    
                    
                   
                    
                }
            }

        }

        private ObservableCollection<TodoItem> LoadTasks()
        {
            // Ensure the directory exists
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_dataFilePath));

            if (System.IO.File.Exists(_dataFilePath))
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(_dataFilePath);
                    // Deserialize the JSON string back into the ObservableCollection
                    var loadedList = JsonSerializer.Deserialize<ObservableCollection<TodoItem>>(jsonString);

                    // If deserialization was successful, return the loaded list
                    if (loadedList != null)
                    {
                        return loadedList;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Fallback to a new empty list if loading fails
                }
            }

            // Return a new list if the file doesn't exist or loading failed
            return new ObservableCollection<TodoItem>();
        }

        private ObservableCollection<string> LoadFilters()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_filterFilePath));
            if (System.IO.File.Exists(_filterFilePath))
            {
                try
                {
                    string jsonString = System.IO.File.ReadAllText(_filterFilePath);
                    var loadedList = JsonSerializer.Deserialize<ObservableCollection<string>>(jsonString);
                    if (loadedList != null)
                    {
                        return loadedList;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading filter data: {ex.Message}", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Fallback to a new empty list if loading fails
                }
            }
            return new ObservableCollection<string>();
        }


        private void ExecuteSaveCommand(object parameter)
        {
            try
            {
                // Serialize the entire collection into a JSON string
                string jsonString = JsonSerializer.Serialize(TodoList, new JsonSerializerOptions { WriteIndented = true });

                // Write the JSON string to the file
                File.WriteAllText(_dataFilePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Data Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFilters()
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_filterFilePath));
                // Serialize the AvailableOptions collection into a JSON string
                string jsonString = JsonSerializer.Serialize(AvailableOptions, new JsonSerializerOptions { WriteIndented = true });
                // Write the JSON string to the filter file
                File.WriteAllText(_filterFilePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving filters: {ex.Message}", "Filter Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}