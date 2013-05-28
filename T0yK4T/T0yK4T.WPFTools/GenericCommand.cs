using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace T0yK4T.WPFTools
{
    /// <summary>
    /// A generic implementation of <see cref="ICommand"/>
    /// </summary>
    /// <typeparam name="TExecutorArg">The type of argument that the executor will receive</typeparam>
    /// <typeparam name="TCheckerArg">The type of argument that the "checker" will receive <see cref="CanExecute(object)"/></typeparam>
    public class GenericCommand<TExecutorArg, TCheckerArg> : ICommand
    {
        private Action<TExecutorArg> executor;
        private Predicate<TCheckerArg> canExecute;

        /// <summary>
        /// Initializes a new instance of <see cref="GenericCommand{TExecutorArgs,TCheckerArgs}"/>
        /// <para/>
        /// Note that the checker will be set to null, and thus will not be run (<see cref="CanExecute(object)"/> will return true)
        /// </summary>
        /// <param name="executor">The executor that will be called when the <see cref="Execute(object)"/> method is called</param>
        public GenericCommand(Action<TExecutorArg> executor)
            : this(executor, null) { }

        /// <summary>
        /// Initializes a new instance of <see cref="GenericCommand{TExecutorArgs,TCheckerArgs}"/>
        /// </summary>
        /// <param name="executor">The executor that will be called when the <see cref="Execute(object)"/> method is called</param>
        /// <param name="canExecute">The checker that will be run when <see cref="CanExecute(object)"/> is called</param>
        public GenericCommand(Action<TExecutorArg> executor, Predicate<TCheckerArg> canExecute)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");
            this.executor = executor;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Returns a value indicating whether or not this command can currently be executed
        /// <para/>
        /// <remarks>
        /// Note that this method will return true by default if no checker delegate has been set
        /// </remarks>
        /// </summary>
        /// <param name="parameter">The parameter for the checker</param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            TCheckerArg arg;
            try { arg = (TCheckerArg)parameter; }
            catch (InvalidCastException) { throw new NotSupportedException("The specified parameter is not valid"); }
            return this.canExecute == null ? true : this.canExecute(arg);
        }

        /// <summary>
        /// <see cref="ICommand.CanExecuteChanged"/>
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Executes the executor with the specified parameter as argument
        /// </summary>
        /// <param name="parameter">The argument to pass to the command</param>
        public void Execute(object parameter)
        {
            TExecutorArg arg;
            try { arg = (TExecutorArg)parameter; }
            catch (InvalidCastException) { throw new NotSupportedException("The specified parameter is not valid"); }
            this.executor(arg);
        }
    }
}
