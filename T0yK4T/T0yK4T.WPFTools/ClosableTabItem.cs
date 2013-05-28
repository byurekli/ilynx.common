

using System.Windows.Controls;
using System.Windows;
using System;
namespace T0yK4T.WPFTools
{
    public class ClosableTabItem : TabItem
    {
        public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClosableTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { base.AddHandler(CloseTabEvent, value); }
            remove { base.RemoveHandler(CloseTabEvent, value); }
        }

        static ClosableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTabItem), new FrameworkPropertyMetadata(typeof(ClosableTabItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Button closeButton = base.Template.FindName("PART_Close", this) as Button;
            if (closeButton != null)
                closeButton.Click += new RoutedEventHandler(closeButton_Click);
        }

        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }
    }
}