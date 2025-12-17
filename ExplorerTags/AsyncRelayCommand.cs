using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading; // Required for WPF Dispatcher

namespace ExplorerTags
{
    public class AsyncRelayCommand : ICommand
    {
        // The asynchronous action to execute
        private readonly Func<object, Task> _execute;

        // The optional function to determine if the command can execute
        private readonly Func<object, bool> _canExecute;

        // Tracks if the command is currently running
        private volatile bool _isExecuting;

        // -------------------------------------------------------------------------
        // CONSTRUCTOR
        // -------------------------------------------------------------------------
        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // -------------------------------------------------------------------------
        // ICOMMAND MEMBERS
        // -------------------------------------------------------------------------

        // The CanExecuteChanged event must be raised on the UI thread
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Checks if the command can be executed. It also checks if the command is already running.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            // If the command is already running, it cannot execute again.
            if (_isExecuting)
                return false;

            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the asynchronous command logic.
        /// </summary>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged(); // Signal that the command is now disabled

                // Execute the async Task method
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged(); // Signal that the command is now enabled again
            }
        }

        // -------------------------------------------------------------------------
        // HELPER METHOD
        // -------------------------------------------------------------------------

        /// <summary>
        /// Forces the CommandManager to check CanExecute on all bound commands.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            // CommandManager.InvalidateRequerySuggested() internally raises the CanExecuteChanged event 
            // using the UI thread dispatcher, ensuring thread safety.
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
