using System.Linq;
using System.Windows;
using T0yK4T.Tools;
using T0yK4T.Threading;
using System;
using System.Windows.Input;

namespace Hasherer
{
    /// <summary>
    /// WPF Proxy class for <see cref="AsyncHashProvider"/>, used for databinding
    /// </summary>
    public class HashProviderProxy : DependencyObject
    {
        /// <summary>
        /// The Name property
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(HashProviderProxy));

        /// <summary>
        /// The Progress property
        /// </summary>
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(HashProviderProxy));

        /// <summary>
        /// The Result property
        /// </summary>
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register("Result", typeof(string), typeof(HashProviderProxy));

        /// <summary>
        /// The IsEnabled property
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(HashProviderProxy), new PropertyMetadata(true));

        /// <summary>
        /// The internal hash provider that this <see cref="HashProviderProxy"/> wraps
        /// </summary>
        private AsyncHashProvider provider;

        /// <summary>
        /// Initializes a new instance of <see cref="HashProviderProxy"/> and wraps around the specified <see cref="AsyncHashProvider"/>
        /// </summary>
        /// <param name="provider">The provider to wrap around</param>
        public HashProviderProxy(AsyncHashProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            this.provider = provider;
            this.Name = provider.Name;
            this.Result = "N/A";
            this.provider.Progress += new GenericEventHandler<ProgressWorker<HashInputArgs, HashOutputArgs>, double>(provider_Progress);
            this.provider.WorkStarted += new GenericEventHandler<ThreadedWorker<HashInputArgs, HashOutputArgs>>(provider_WorkStarted);
            this.provider.WorkCompleted += new GenericEventHandler<ThreadedWorker<HashInputArgs, HashOutputArgs>, HashOutputArgs>(provider_WorkCompleted);
            this.provider.WorkFailed += new GenericEventHandler<ThreadedWorker<HashInputArgs, HashOutputArgs>, Exception>(provider_WorkFailed);
            this.provider.WorkAborted += new GenericEventHandler<ThreadedWorker<HashInputArgs, HashOutputArgs>>(provider_WorkAborted);
        }

        void provider_WorkStarted(ThreadedWorker<HashInputArgs, HashOutputArgs> sender)
        {
            this.Result = "Working";
        }

        /// <summary>
        /// Starts the provider
        /// </summary>
        /// <param name="args">The <see cref="HashInputArgs"/> to pass to the provider's Execute method</param>
        public void Start(HashInputArgs args)
        {
            this.showDots = true;
            this.lastProgress = -1d;
            this.Progress = 0d;
            if (this.IsEnabled)
            {
                try { this.provider.Execute(args); }
                catch { }
            }
            else
                this.Result = "N/A";
        }

        /// <summary>
        /// Aborts the provider
        /// </summary>
        public void Abort()
        {
            this.showDots = false;
            this.lastProgress = -1d;
            this.Progress = 0d;
            this.provider.Abort();
        }

        void provider_WorkAborted(ThreadedWorker<HashInputArgs, HashOutputArgs> sender)
        {
            this.showDots = false;
            this.lastProgress = -1d;
            this.Progress = 0d;
            this.Result = "Aborted";
        }

        void provider_WorkFailed(ThreadedWorker<HashInputArgs, HashOutputArgs> sender, Exception val)
        {
            this.showDots = false;
            this.lastProgress = -1d;
            this.Progress = 0d;
            this.Result = val.ToString();
        }

        void provider_WorkCompleted(ThreadedWorker<HashInputArgs, HashOutputArgs> sender, HashOutputArgs val)
        {
            this.showDots = false;
            this.Result = BitConverter.ToString(val.Hash);
            this.lastProgress = -1d;
            this.Progress = 100d;
        }

        void provider_Progress(ProgressWorker<HashInputArgs, HashOutputArgs> sender, double val)
        {
            this.Progress = val;
        }

        private new bool CheckAccess()
        {
            return base.CheckAccess();
        }

        /// <summary>
        /// Overriden to to abort provider if enabled changed to false
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsEnabledProperty)
            {
                if (!(bool)e.NewValue)
                    this.provider.Abort();
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating wether or not this hashprovider proxy is enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)base.GetValue(IsEnabledProperty); }
            set
            {
                if (!this.CheckAccess())
                    base.Dispatcher.Invoke(new Action<bool>(b => this.IsEnabled = b), value);
                else
                    base.SetValue(IsEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets the hash name of the provider
        /// </summary>
        public string Name
        {
            get
            {
                return (string)base.GetValue(NameProperty);
            }
            private set
            {
                if (!this.CheckAccess())
                    base.Dispatcher.Invoke(new Action<string>(s => this.Name = s), value);
                else
                    base.SetValue(NameProperty, value);
            }
        }

        double lastProgress = 0.0d;
        int dots = 0;
        private bool showDots = true;

        /// <summary>
        /// Gets the current progress of the provider
        /// </summary>
        public double Progress
        {
            get
            {
                return (double)base.GetValue(ProgressProperty);
            }
            private set
            {
                if (value - lastProgress < 0.5)
                    return;
                if (!this.CheckAccess())
                    base.Dispatcher.Invoke(new Action<double>(d => this.Progress = d), value);
                else
                {
                    if (this.showDots)
                    {
                        dots = dots >= 3 ? 0 : dots + 1;
                        this.Result = string.Format("Working{0}", this.GetDots(dots));
                    }
                    lastProgress = value;
                    base.SetValue(ProgressProperty, value);
                }
            }
        }

        private string GetDots(int dots)
        {
            string s = string.Empty;
            for (int i = 0; i < dots; ++i)
                s += ".";
            return s;
        }

        /// <summary>
        /// Gets the hash result (as a string) of the current provider
        /// </summary>
        public string Result
        {
            get
            {
                return (string)base.GetValue(ResultProperty);
            }
            private set
            {
                if (!this.CheckAccess())
                    base.Dispatcher.Invoke(new Action<string>(s => this.Result = s), value);
                else
                    base.SetValue(ResultProperty, value);
            }
        }
    }
}