using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MessageCloud.Commanding
{
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.  This class allows 
    /// you to accept command parameters in the
    /// Execute and CanExecute callback methods.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region RaiseCanExecuteChanged

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RaiseCanExecuteChanged

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            Debug.Assert(parameter == null || parameter is T, "Invalid parameter object type.  Expected type: " + typeof(T).FullName + "  Actual type: " + parameter.GetType().FullName);

            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion // ICommand Members
    }

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.  This class does not allow
    /// you to accept command parameters in the
    /// Execute and CanExecute callback methods.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action _execute;
        readonly Func<bool> _canExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region RaiseCanExecuteChanged

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RaiseCanExecuteChanged

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute();
        }

        #endregion // ICommand Members
    }
}