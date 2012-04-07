using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Security.Cryptography;
using T0yK4T.Threading;
using System.Collections.ObjectModel;
using T0yK4T.Tools;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Timers;
using T0yK4T.WPFTools;

namespace Hasherer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BorderlessWindow
    {
        private ObservableCollection<HashProviderProxy> providers = new ObservableCollection<HashProviderProxy>();
        private ObservableCollection<HashProviderProxy> Providers
        {
            get { return this.providers; }
        }

        private ObservableCollection<Encoding> encodings = new ObservableCollection<Encoding>();
        private ObservableCollection<Encoding> Encodings
        {
            get { return this.encodings; }
        }

        /// <summary>
        /// The open file command
        /// </summary>
        public static readonly ICommand OpenFileCommand = new GenericCommand<MainWindow, object>(o =>
        {
            o.HideRaw();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
                o.LoadFile(dlg.FileName);
        });

        /// <summary>
        /// Hides the raw input box
        /// </summary>
        public void HideRaw()
        {
            this.PART_rawToggle.IsChecked = false;
        }

        /// <summary>
        /// ...
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.infoBox.ItemsSource = this.InfoCollection;
            this.hashList.ItemsSource = this.Providers;
            List<Encoding> l = new List<Encoding>(Encoding.GetEncodings().Select<EncodingInfo, Encoding>(ei => Encoding.GetEncoding(ei.CodePage)));
            l.Sort(new Comparison<Encoding>((e, e2) => string.Compare(e.EncodingName, e2.EncodingName)));
            this.encodings = new ObservableCollection<Encoding>(l);
            this.encodingBox.ItemsSource = this.Encodings;
            this.encodingBox.SelectedIndex = this.encodings.IndexOf(this.encodings.FirstOrDefault(e => e.EncodingName == "Unicode"));
        }

        /// <summary>
        /// Overridden to load hash providers!
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RuntimeCommon.MainLogger = this.logBox;
            AssemblyLoader loader = new AssemblyLoader();
            foreach (IProviderInstantiator providerInstantiator in loader.LoadDirectory<IProviderInstantiator>(Environment.CurrentDirectory))
                this.providers.Add(new HashProviderProxy(providerInstantiator.Instantiate()) { IsEnabled = providerInstantiator.DefaultEnabled });
            this.LoadFile(Process.GetCurrentProcess().MainModule.FileName);
        }

        /// <summary>
        /// Overridden to ensure that no filehandles are left open (calls <see cref="HashProviderProxy.Abort()"/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            foreach (HashProviderProxy proxy in this.providers)
                proxy.Abort();
        }

        private new bool CheckAccess() { return base.CheckAccess(); }

        private void hashList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            MouseWheelEventArgs args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            args.RoutedEvent = UIElement.MouseWheelEvent;
            this.viewer.RaiseEvent(args);
        }

        /// <summary>
        /// Loads the file located at the specified path if it exists
        /// </summary>
        /// <param name="fName"></param>
        public void LoadFile(string fName)
        {
            if (!File.Exists(fName))
                return;
            this.RestartAll(new HashInputArgs(fName));
            this.SetInfo(new FileInfo(fName));
        }

        private ThreadedWorker<WorkerArgs, CompletedArgs> encodingWorker = ThreadedWorker<WorkerArgs, CompletedArgs>.Create(w => new CompletedArgs { Raw = w.Encoding.GetBytes(w.Text), Encoding = w.Encoding, Text = w.Text });

        private void LoadText(string text)
        {
            Encoding enc;
            if ((enc = this.encodingBox.SelectedItem as Encoding) != null)
            {
                encodingWorker.Abort();
                encodingWorker.Execute(new WorkerArgs
                {
                    Encoding = enc,
                    Text = text
                }, new Action<CompletedArgs>(a =>
                    {
                        base.Dispatcher.Invoke(new Action<CompletedArgs>(a2 =>
                        {
                            this.RestartAll(new HashInputArgs(a2.Raw));
                            this.SetTextInfo(a2.Encoding, a2.Raw, a2.Text);
                        }), a);
                    }));
            }
        }

        private struct WorkerArgs
        {
            public string Text;
            public Encoding Encoding;
        }

        private struct CompletedArgs
        {
            public byte[] Raw;
            public string Text;
            public Encoding Encoding;
        }

        private void SetTextInfo(Encoding enc, byte[] textData, string text)
        {
            this.infoCollection.Clear();
            this.currentInfos.Clear();
            this.currentInfos.Add(new BindableKeyValuePair("Encoding", enc.EncodingName));
            //this.currentInfos.Add(new BindableKeyValuePair("Text", text));
            //this.currentInfos.Add(new BindableKeyValuePair("Raw", textData.ToString("-")));
            Timer t = new Timer(50d);
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        private void RestartAll(HashInputArgs args)
        {
            foreach (HashProviderProxy proxy in this.providers)
            {
                proxy.Abort();
                proxy.Start(args);
            }
        }

        private ObservableCollection<BindableKeyValuePair> infoCollection = new ObservableCollection<BindableKeyValuePair>();
        private ObservableCollection<BindableKeyValuePair> InfoCollection
        {
            get { return this.infoCollection; }
        }

        private List<BindableKeyValuePair> currentInfos = new List<BindableKeyValuePair>();
        private void SetInfo(FileInfo info)
        {
            this.infoCollection.Clear();
            this.currentInfos.Clear();
            this.currentInfos.Add(new BindableKeyValuePair("File", info.Name));
            this.currentInfos.Add(new BindableKeyValuePair("Size", string.Format("{0:f2} MiB", ((double)info.Length / 1024d) / 1024d)));
            this.currentInfos.Add(new BindableKeyValuePair("Path", info.FullName));
            this.currentInfos.Add(new BindableKeyValuePair("Directory", info.Directory));
            this.currentInfos.Add(new BindableKeyValuePair("Extension", info.Extension));
            this.currentInfos.Add(new BindableKeyValuePair("Attributes", info.Attributes));
            this.currentInfos.Add(new BindableKeyValuePair("Creation Time", info.CreationTime));
            this.currentInfos.Add(new BindableKeyValuePair("Read Only", info.IsReadOnly));
            this.currentInfos.Add(new BindableKeyValuePair("Last Accessed", info.LastAccessTime));
            this.currentInfos.Add(new BindableKeyValuePair("Last Write", info.LastWriteTime));
            Timer t = new Timer(50d);
            //t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            base.Dispatcher.Invoke(new Action<Timer>(t =>
            {
                if (this.currentInfos.Count == 0)
                {
                    t.Stop();
                    return;
                }
                else
                {
                    this.InfoCollection.Add(this.currentInfos[0]); this.currentInfos.RemoveAt(0);
                    t.Start();
                }
            }), (Timer)sender);
        }

        private void cleaLogBtn_Click(object sender, RoutedEventArgs e)
        {
            this.logBox.Clear();
        }

        private void rawOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.LoadText(this.rawBox.Text);
        }
    }
}
