using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using T0yK4T.WPFTools;
using System.Windows.Input;
using System.Windows.Controls;

namespace Hasherer
{
    public class BorderLessWindow : Window
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(UIElement), typeof(BorderLessWindow));
        Point offset;
        private bool mouseDown = false;

        public UIElement Header
        {
            get { return this.GetValueSafe<UIElement>(HeaderProperty); }
            set { this.SetValueSafe(HeaderProperty, value); }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            this.mouseDown = true;
            this.offset = e.GetPosition(this);
            base.OnMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (this.mouseDown)
            {
                Point current = this.PointToScreen(e.GetPosition(this));
                this.Top = current.Y - this.offset.Y;
                this.Left = current.X - this.offset.X;
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            this.mouseDown = false;
            if (Mouse.Captured == this)
                Mouse.Capture(null);
            base.OnPreviewMouseUp(e);
        }
    }

    public static class Commands
    {
        /// <summary>
        /// Closes a window
        /// </summary>
        public static readonly ICommand CloseWindowCommand = new GenericCommand<Window, object>(w =>
        {
            if (w != null)
                w.Close();
        });

        public static readonly ICommand MinimizeWindowCommand = new GenericCommand<Window, object>(w =>
        {
            if (w != null)
                w.WindowState = WindowState.Minimized;
        });

        public static readonly ICommand MaximizeBorderlessCommand = new GenericCommand<Window, object>(w =>
        {
            if (w != null)
            {
                //SystemParameters.VirtualScreenHeight
                if (w.WindowState == WindowState.Maximized)
                    w.WindowState = WindowState.Normal;
                else
                    w.WindowState = WindowState.Maximized;
            }
        });
    }
}
