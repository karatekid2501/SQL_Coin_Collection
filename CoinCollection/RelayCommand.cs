using System.Windows.Input;

namespace CoinCollection
{
    //https://stackoverflow.com/questions/22285866/why-relaycommand

    /// <summary>
    /// Relay command for XAML windows
    /// </summary>
    /// <typeparam name="T">Type of the command</typeparam>
    internal class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, null!)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        
        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || parameter is T param && _canExecute(param);
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object? parameter)
        {
            if(parameter is T param)
            {
                _execute(param);
            }
            else if(parameter == null && default(T) == null)
            {
                _execute(default!);
            }
            else
            {
                throw new InvalidOperationException($"Invalid parameter type. Expected {typeof(T)}, but got {parameter?.GetType()}.");
            }
        }

        #endregion
    }

    /// <summary>
    /// Basic relay command for XAML window
    /// </summary>
    /// <param name="execute">Basic action to execute</param>
    internal class RelayCommand(Action execute) : RelayCommand<object>(_ => execute())
    {
    }
}
