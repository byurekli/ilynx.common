using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using T0yK4T.WPFTools;
using System.Windows.Input;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;

namespace T0yK4T.WPFTools
{
    /// <summary>
    /// Backing class for a borderless window
    /// </summary>
    public class BorderlessWindow : Window
    {
        /// <summary>
        /// The header property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(UIElement), typeof(BorderlessWindow));

        private Point offset;
        private bool mouseDown = false;
        private const int WM_SYSCOMMAND = 0x112;
        private WindowInteropHelper helper;
        private List<Rectangle> borderRects = new List<Rectangle>();
        private ResizeDirection resizeDirection;
        private Button maximizeButton;
        private Rectangle moveGrip;

        static BorderlessWindow()
        {
            Window.DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderlessWindow), new FrameworkPropertyMetadata(typeof(BorderlessWindow)));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BorderlessWindow"/>
        /// </summary>
        public BorderlessWindow() { this.helper = new WindowInteropHelper(this); }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        /// <summary>
        /// Overriden to get references, this probably needs refactoring
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.borderRects.Add((Rectangle)base.Template.FindName("Top", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("Left", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("Right", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("Bottom", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("TopLeft", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("TopRight", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("BottomLeft", this));
            this.borderRects.Add((Rectangle)base.Template.FindName("BottomRight", this));

            foreach (Rectangle rect in this.borderRects)
            {
                if (rect != null)
                {
                    rect.MouseDown += new MouseButtonEventHandler(rect_MouseDown);
                    rect.MouseMove += new MouseEventHandler(rect_MouseMove);
                }
            }

            this.maximizeButton = (Button)base.Template.FindName("maximizeButton", this);
            if (maximizeButton != null)
                maximizeButton.Content = "1";

            this.moveGrip = (Rectangle)base.Template.FindName("moveGrip", this);
            //this.headerElement = (ContentControl)base.Template.FindName("headerControl", this);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == Window.WindowStateProperty)
            {
                if (this.maximizeButton != null)
                {
                    WindowState val = (WindowState)e.NewValue;
                    if (val == WindowState.Maximized)
                        this.maximizeButton.Content = "2";
                    else
                        this.maximizeButton.Content = "1";
                }
            }
        }

        void rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                SendMessage(this.helper.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + this.resizeDirection), IntPtr.Zero);
        }

        void rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect != null)
            {
                try { this.resizeDirection = (ResizeDirection)Enum.Parse(typeof(ResizeDirection), rect.Name); }
                catch { }
            }
        }

        /// <summary>
        /// Gets or Sets the header of this window
        /// </summary>
        public UIElement Header
        {
            get { return this.GetValueSafe<UIElement>(HeaderProperty); }
            set { this.SetValueSafe(HeaderProperty, value); }
        }

        /// <summary>
        /// Overriden to capture mouse for movement purposes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (VisualTreeHelper.HitTest(this.moveGrip, e.GetPosition(this.moveGrip)) != null)
            {
                Mouse.Capture(this);
                this.mouseDown = true;
                this.offset = e.GetPosition(this);
            }
        }

        /// <summary>
        /// Moving the window
        /// </summary>
        /// <param name="e"></param>
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

        /// <summary>
        /// Releasing mouse capture
        /// </summary>
        /// <param name="e"></param>
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
                if (w.WindowState == WindowState.Maximized)
                    w.WindowState = WindowState.Normal;
                else
                    w.WindowState = WindowState.Maximized;
            }
        });
    }
}
