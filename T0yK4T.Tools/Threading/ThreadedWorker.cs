using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace T0yK4T.Tools.Threading
{
    /// <summary>
    /// An interface describing a progress display (I suppose...)
    /// </summary>
    public interface ISupportProgress
    {
        /// <summary>
        /// Used to update only the progress value
        /// </summary>
        /// <param name="source">The source "worker" who sent this message</param>
        /// <param name="percent"></param>
        void Progress(Guid source, double percent);

        /// <summary>
        /// Used to update both the progress value and display-text
        /// </summary>
        /// <param name="source">The source "worker" who sent this message</param>
        /// <param name="text"></param>
        /// <param name="percent"></param>
        void Progress(Guid source, string text, double percent);

        /// <summary>
        /// Used to update only the progress text
        /// </summary>
        /// <param name="source">The source "worker" who sent this message</param>
        /// <param name="text"></param>
        void Progress(Guid source, string text);

        /// <summary>
        /// Used to set the title of the progress display
        /// </summary>
        /// <param name="title"></param>
        void SetTitle(string title);
    }

    /// <summary>
    /// Helper class for executing work asynchronously (Read, in a seperate thread)
    /// </summary>
    /// <typeparam name="TArgs">The type of arguments that are passed to the worker thread</typeparam>
    /// <typeparam name="TCompletedArgs">The expected output type</typeparam>
    public abstract class ThreadedWorker<TArgs, TCompletedArgs>
    {
        /// <summary>
        /// Set internally by <see cref="Execute(TArgs, Action{TCompletedArgs})"/>
        /// </summary>
        private Thread worker;

        private Guid id;
        private Action<TCompletedArgs> workCompleteCallback;
        private ISupportProgress progressDisplay;
        private SynchronizationContext context;
        
        /// <summary>
        /// Initializes a new instance of ThreadedWorker and sets the internal ID field to the specified value
        /// </summary>
        /// <param name="ID"></param><param name="progressDisplay"></param>
        public ThreadedWorker(Guid ID, ISupportProgress progressDisplay)
        {
            this.id = ID;
            this.progressDisplay = progressDisplay;
        }

        /// <summary>
        /// Gets a value indicating the ID of this worker (assigned during construction
        /// </summary>
        protected Guid ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the <see cref="Action{T1}"/> that should be used to indicate work completion
        /// </summary>
        protected Action<TCompletedArgs> WorkCompleteCallback
        {
            get { return this.workCompleteCallback; }
        }

        /// <summary>
        /// Executes this worker
        /// </summary>
        /// <param name="args">The arguments that are passed on to the thread</param>
        /// <param name="workCompleteCallback">The method to call when the work has completed</param>
        public void Execute(TArgs args, Action<TCompletedArgs> workCompleteCallback)
        {
            this.context = SynchronizationContext.Current;
            if (this.context == null)
                this.context = new SynchronizationContext();

            this.workCompleteCallback = workCompleteCallback;
            this.worker = new Thread(new ParameterizedThreadStart(this.DoWork));
            this.worker.Start(args);
        }

        /// <summary>
        /// Implementors can call this when the work is completed
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCompleted(TCompletedArgs args)
        {
            this.context.Post(new SendOrPostCallback((o) =>
            {
                if (this.workCompleteCallback != null)
                    this.workCompleteCallback.Invoke((TCompletedArgs)o);
            }), args);
        }

        /// <summary>
        /// Implementors can call this method to update progress on the internal <see cref="ISupportProgress"/> object
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void OnProgress(double progress)
        {
            this.context.Post(new SendOrPostCallback((o) =>
            {
                if (this.progressDisplay != null)
                    this.progressDisplay.Progress(this.ID, (double)progress);
            }), progress);
        }

        /// <summary>
        /// Used to set the title of the progress form
        /// </summary>
        /// <param name="title"></param>
        protected virtual void SetProgressTitle(string title)
        {
            this.context.Post(new SendOrPostCallback((o) =>
            {
                if (this.progressDisplay != null)
                    this.progressDisplay.SetTitle((string)o);
            }), title);
        }

        /// <summary>
        /// Implementors can call this method to update progress on the internal <see cref="ISupportProgress"/> object
        /// </summary>
        /// <param name="text">The text to display</param>
        protected virtual void OnProgress(string text)
        {
            this.context.Post(new SendOrPostCallback((o) =>
            {
                if (this.progressDisplay != null)
                    this.progressDisplay.Progress(this.id, (string)o);
            }), text);
        }

        /// <summary>
        /// Implementors can call this method to update progress on the internal <see cref="ISupportProgress"/> object
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="text"></param>
        protected virtual void OnProgress(string text, double progress)
        {
            this.context.Post(new SendOrPostCallback((o) =>
            {
                if (this.progressDisplay != null)
                {
                    FullProgress fp = (FullProgress)o;
                    this.progressDisplay.Progress(this.id, fp.Text, fp.Progress);
                }
            }), new FullProgress { Text = text, Progress = progress });
        }

        private struct FullProgress
        {
            public string Text;
            public double Progress;
        }

        /// <summary>
        /// This method is the entry point of the worker thread
        /// </summary>
        /// <param name="args">Will contain a <typeparamref name="TArgs"/> object as an object</param>
        protected abstract void DoWork(object args);
    }
}
