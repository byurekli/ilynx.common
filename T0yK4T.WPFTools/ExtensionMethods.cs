

using System.Windows.Threading;
using System.Windows;
using System;
using System.Threading;
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
    }
}