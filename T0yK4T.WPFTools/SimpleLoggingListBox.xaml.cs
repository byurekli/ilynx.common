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
using T0yK4T.Tools;

namespace T0yK4T.WPFTools
{
    /// <summary>
    /// A simple usercontrol that can be used to log to a listbox
    /// </summary>
    public partial class SimpleLoggingListBox : UserControl, ILogger
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SimpleLoggingListBox"/>
        /// </summary>
        public SimpleLoggingListBox()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            if (!this.CheckAccess())
                base.Dispatcher.Invoke(new Action(() => this.Clear()), null);
            else
                this.box.Items.Clear();
        }

        /// <summary>
        /// <see cref="ILogger.Log(LoggingType,object,string)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void Log(LoggingType type, object sender, string message)
        {
            this.WriteLine(string.Format("[{1}][{0}]: {2}", DateTime.Now, type.ToString()[0], message));
        }

        private new bool CheckAccess()
        {
            return base.CheckAccess();
        }

        private void WriteLine(string line)
        {
            if (!this.CheckAccess())
                base.Dispatcher.Invoke(new Action<string>(s => this.WriteLine(s)), line);
            else
            {
                ListBoxItem item = new ListBoxItem { Content = line };
                this.box.Items.Add(item);
                ScrollViewer viewer = this.box.FindVisualChild<ScrollViewer>();
                if (viewer != null)
                {
                    viewer.ScrollToBottom();
                    viewer.ScrollToLeftEnd();
                }
            }
        }
    }
}
