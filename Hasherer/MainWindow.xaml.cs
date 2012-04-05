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

namespace Hasherer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BorderLessWindow
    {
        private ObservableCollection<HashProviderProxy> providers = new ObservableCollection<HashProviderProxy>();

        /// <summary>
        /// ...
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.infoBox.ItemsSource = this.InfoCollection;
            Binding providerBinding = new Binding();
            providerBinding.Source = providers;
            providerBinding.Mode = BindingMode.OneWay;
            this.hashList.SetBinding(ListBox.ItemsSourceProperty, providerBinding);
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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
                this.LoadFile(dlg.FileName);
        }

        private void LoadFile(string fName)
        {
            if (!File.Exists(fName))
                return;
            HashInputArgs args = new HashInputArgs(fName);
            this.SetInfo(new FileInfo(fName));
            foreach (HashProviderProxy proxy in this.providers)
            {
                proxy.Abort();
                proxy.Start(args);
            }
        }

        private ObservableCollection<BindableKeyValuePair> infoCollection = new ObservableCollection<BindableKeyValuePair>();
        public ObservableCollection<BindableKeyValuePair> InfoCollection
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
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.currentInfos.Count == 0)
            {
                ((Timer)sender).AutoReset = false;
                ((Timer)sender).Stop();
                return;
            }
            base.Dispatcher.BeginInvoke(new Action<BindableKeyValuePair>(bk => this.InfoCollection.Add(bk)), this.currentInfos[0]);
            this.currentInfos.RemoveAt(0);
        }

        private void cleaLogBtn_Click(object sender, RoutedEventArgs e)
        {
            this.logBox.Clear();
        }
    }
}
