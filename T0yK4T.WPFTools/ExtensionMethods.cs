

using System.Windows.Threading;
using System.Windows;
using System;
using System.Threading;
using System.Windows.Media;
namespace T0yK4T.WPFTools
{
    public static class ExtensionMethods
    {
        public static void SetValueSafe(this DependencyObject obj, DependencyProperty property, object value)
        {
            if (!obj.CheckAccess())
                obj.Dispatcher.Invoke(new Action<DependencyObject, DependencyProperty, object>((d, p, o) => d.SetValueSafe(p, o)), obj, property, value);
            else
                obj.SetValue(property, value);
        }

        public static T GetValueSafe<T>(this DependencyObject obj, DependencyProperty property, double milliTimeout = 50d)
        {
            object val = null;
            bool done = false;
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(new Action<DependencyObject, DependencyProperty>((d, dp) =>
                {
                    val = d.GetValue(dp);
                    done = true;
                }), TimeSpan.FromMilliseconds(milliTimeout), obj, property);
            }
            else
                return (T)obj.GetValue(property);
            
            while (!done)
                Thread.Sleep(0);

            return (T)val;
        }

        public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
        {
            T ret = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); ++i)
            {
                DependencyObject c = VisualTreeHelper.GetChild(obj, i);
                if (c != null)
                {
                    if (c is T)
                        ret = (T)c;
                    else
                        ret = c.FindVisualChild<T>();
                    if (ret != null)
                        break;
                }
            }
            return ret;
        }
    }
}