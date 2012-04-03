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

namespace Hasherer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<HashProviderProxy> providers = new ObservableCollection<HashProviderProxy>();

        /// <summary>
        /// ...
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
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
            foreach (AsyncHashProvider provider in loader.LoadDirectory<IProviderInstantiator>(Environment.CurrentDirectory).Aggregate<IProviderInstantiator, List<AsyncHashProvider>>(new List<AsyncHashProvider>(), (l, ip) => { l.AddRange(ip.Instantiate()); return l; }))
                this.providers.Add(new HashProviderProxy(provider));
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

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                HashInputArgs args = new HashInputArgs(dlg.FileName);
                foreach (HashProviderProxy proxy in this.providers)
                {
                    proxy.Abort();
                    proxy.Start(args);
                }
            }
        }
    }
}
