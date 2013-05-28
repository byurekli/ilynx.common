using System.Windows.Controls;
using T0yK4T.Tools;
using System;
using System.Windows;

namespace T0yK4T.WPFTools
{
    /// <summary>
    /// Simple logging textbox, implemetns <see cref="ILogger"/>
    /// </summary>
    public partial class SimpleLoggingTextBox : UserControl, ILogger
    {
        static SimpleLoggingTextBox()
        {
            ScrollViewer.HorizontalScrollBarVisibilityProperty.OverrideMetadata(typeof(SimpleLoggingTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));
            ScrollViewer.VerticalScrollBarVisibilityProperty.OverrideMetadata(typeof(SimpleLoggingTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));
        }

        /// <summary>
        /// Default constructor, sets up a few thingies
        /// </summary>
        public SimpleLoggingTextBox()
        {
            InitializeComponent();
            this.box.IsReadOnly = true;
        }

        /// <summary>
        /// Clears the contents of the text box
        /// </summary>
        public void Clear()
        {
            if (!this.CheckAccess())
                base.Dispatcher.Invoke(new Action(() => this.Clear()), null);
            else
                this.box.Text = string.Empty;
        }

        /// <summary>
        /// <see cref="ILogger.Log(LoggingType,object,string)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void Log(LoggingType type, object sender, string message)
        {
            this.WriteLine(string.Format("[{0}:{1}]: {2}", type.ToString()[0], sender.GetType().FullName, message));
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
                this.box.Text += line + Environment.NewLine;
        }
    }

}