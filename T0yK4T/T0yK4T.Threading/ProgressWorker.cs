
using System;
using T0yK4T.Tools;
namespace T0yK4T.Threading
{
    /// <summary>
    /// This class can be used to run an asynchronous operation while also providing progress updates to users
    /// </summary>
    /// <typeparam name="TArgs">The type of arguments this worker will use</typeparam>
    /// <typeparam name="TCompletedArgs">The type of results the worker will produce</typeparam>
    public abstract class ProgressWorker<TArgs, TCompletedArgs> : ThreadedWorker<TArgs, TCompletedArgs>
    {
        private Guid id;
        private DateTime lastUpdate = DateTime.Now;

        /// <summary>
        /// This event is fired when the underlying worker has progress in the form percent to report
        /// </summary>
        public event GenericEventHandler<ProgressWorker<TArgs, TCompletedArgs>, double> Progress;

        /// <summary>
        /// This event is fired when the underlying worker has progress in the form of a message to report
        /// </summary>
        public event GenericEventHandler<ProgressWorker<TArgs, TCompletedArgs>, string> Status;

        /// <summary>
        /// Initializes a new instance of <see cref="ProgressWorker{TArgs, TCompletedArgs}"/> and sets the internal ID field to the specified value
        /// </summary>
        /// <param name="ID">The ID of the new worker</param>
        protected ProgressWorker(Guid ID)
        {
            this.id = ID;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ProgressWorker{TArgs, TCompletedArgs}"/> and sets it's worker id to a random Guid
        /// </summary>
        protected ProgressWorker()
            : this(Guid.NewGuid()) { }

        /// <summary>
        /// Used to fire the <see cref="Progress"/> event
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void OnProgress(double progress)
        {
            if (this.Progress != null)
                this.Progress.BeginInvoke(this, progress, new AsyncCallback(iar => this.Progress.EndInvoke(iar)), null);
        }

        /// <summary>
        /// Used to fire the <see cref="Status"/> event
        /// </summary>
        /// <param name="text"></param>
        protected virtual void OnProgress(string text)
        {
            if (this.Status != null)
                this.Status.BeginInvoke(this, text, new AsyncCallback(iar => this.Status.EndInvoke(iar)), null);
        }

        /// <summary>
        /// This method is a convenience method for calling both <see cref="OnProgress(double)"/> and <see cref="OnProgress(string)"/>
        /// <para/>
        /// The two methods are called in the order they are listed above.
        /// </summary>
        /// <param name="progress">The progress to report</param>
        /// <param name="text">The message to report</param>
        protected virtual void OnProgress(string text, double progress)
        {
            this.OnProgress(progress);
            this.OnProgress(text);
        }

        /// <summary>
        /// Gets a value indicating the ID of this worker (assigned during construction)
        /// </summary>
        protected Guid ID
        {
            get { return this.id; }
        }
    }
}