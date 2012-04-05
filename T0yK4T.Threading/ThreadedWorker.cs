using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using T0yK4T.Tools;
using System.Reflection;

namespace T0yK4T.Threading
{
    /// <summary>
    /// Helper class for executing work asynchronously (Read, in a seperate thread)
    /// </summary>
    /// <typeparam name="TArgs">The type of arguments that are passed to the worker thread</typeparam>
    /// <typeparam name="TCompletedArgs">The expected output type</typeparam>
    public abstract class ThreadedWorker<TArgs, TCompletedArgs> : ComponentBase
    {
        /// <summary>
        /// Set internally by <see cref="Execute(TArgs)"/>
        /// </summary>
        private Thread worker;

        /// <summary>
        /// This event is fired when the worker has completed it's work
        /// </summary>
        public event GenericEventHandler<ThreadedWorker<TArgs, TCompletedArgs>, TCompletedArgs> WorkCompleted;

        /// <summary>
        /// This event is fired when the worker has started working
        /// </summary>
        public event GenericEventHandler<ThreadedWorker<TArgs, TCompletedArgs>> WorkStarted;

        /// <summary>
        /// This event is fired if any exceptions are caught during execution
        /// </summary>
        public event GenericEventHandler<ThreadedWorker<TArgs, TCompletedArgs>, Exception> WorkFailed;
        
        /// <summary>
        /// This event is fired when the worker is aborted
        /// </summary>
        public event GenericEventHandler<ThreadedWorker<TArgs, TCompletedArgs>> WorkAborted;

        private bool completed = false;
        private TCompletedArgs result;
        /// <summary>
        /// This property will contain the result of the ThreadedWorker once it has completed execution
        /// </summary>
        public TCompletedArgs Result
        {
            get
            {
                if (!completed)
                {
                    base.LogCritical("Attempted to retrieve result before worker had finished");
                    throw new NotSupportedException("The results cannot be retrieved before the worker has completed executing");
                }
                else
                    return this.result;
            }
        }

        /// <summary>
        /// Executes this worker
        /// </summary>
        /// <param name="args">The arguments that are passed on to the thread</param>
        public void Execute(TArgs args)
        {
            if (this.worker != null && !this.completed)
            {
                base.LogCritical("Attempted to start a running worker");
                throw new NotSupportedException("This worker has already been started and cannot be started again until it has completed, failed or has been aborted");
            }
            else if (this.worker != null && this.completed)
            {
                base.LogDebug("Ensuring worker thread is dead");
                this.Abort();
                this.result = default(TCompletedArgs);
            }
            this.worker = new Thread(new ParameterizedThreadStart(this.DoWork));
            this.worker.Name = this.Name;
            base.LogInformation("Starting worker at {0}", DateTime.Now);
            this.worker.Start(args);
        }

        /// <summary>
        /// Used to invoke the <see cref="WorkStarted"/> event
        /// </summary>
        private void OnStarted()
        {
            base.LogInformation("Worker started at {0}", DateTime.Now);
            if (this.WorkStarted != null)
                this.WorkStarted.BeginInvoke(this, new AsyncCallback(iar => this.WorkStarted.EndInvoke(iar)), null);
        }

        /// <summary>
        /// Used to invoke the <see cref="WorkCompleted"/> event
        /// </summary>
        /// <param name="args">The TCompletedArgs to send in the event</param>
        private void OnCompleted(TCompletedArgs args)
        {
            base.LogInformation("Worker completed at {0}", DateTime.Now);
            if (this.WorkCompleted != null)
                this.WorkCompleted.BeginInvoke(this, args, new AsyncCallback(iar => this.WorkCompleted.EndInvoke(iar)), null);
        }

        /// <summary>
        /// Used to invoke the <see cref="WorkAborted"/> event
        /// </summary>
        private void OnAborted()
        {
            base.LogError("Worker aborted at {0}", DateTime.Now);
            if (this.WorkAborted != null)
                this.WorkAborted.BeginInvoke(this, new AsyncCallback(iar => this.WorkAborted.EndInvoke(iar)), null);
        }

        /// <summary>
        /// Used to invoke the <see cref="WorkFailed"/> event
        /// </summary>
        private void OnFailed(Exception e)
        {
            base.LogError("Worker failed at {0}", DateTime.Now);
            if (this.WorkFailed != null)
                this.WorkFailed.BeginInvoke(this, e, new AsyncCallback(iar => this.WorkFailed.EndInvoke(iar)), null);
        }

        private void DoWork(object args)
        {
            try
            {
                this.OnStarted();
                this.result = this.DoWork((TArgs)args);
                this.completed = true;
                this.OnCompleted(this.result);
            }
            catch (ThreadAbortException) { this.OnAborted(); this.completed = true; }
            catch (Exception e) { this.OnFailed(e); this.completed = true; }
        }

        /// <summary>
        /// Aborts the current worker thread
        /// </summary>
        public void Abort()
        {
            if (this.worker != null)
            {
                try { this.worker.Abort(); }
                catch (Exception e) { base.LogException(e, MethodBase.GetCurrentMethod()); }
            }
        }

        /// <summary>
        /// Gets or Sets the name of this worker
        /// </summary>
        public virtual string Name
        {
            get { return base.ComponentName; }
            protected set { base.ComponentName = value; }
        }

        /// <summary>
        /// This method is the entry point of the worker thread
        /// </summary>
        /// <param name="args">Will contain a <typeparamref name="TArgs"/> object as an object</param>
        protected abstract TCompletedArgs DoWork(TArgs args);
    }
}
