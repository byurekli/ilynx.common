using System.Windows.Data;
using System.Windows;
using System;
using System.Globalization;

namespace T0yK4T.WPFTools
{
    /// <summary>
    /// An extended BooleanToVisibilityConverter that can be "configured"
    /// </summary>
    public class ExtendedBooleanToVisibilityConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        /// The TrueValue property
        /// </summary>
        public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register("TrueValue", typeof(Visibility), typeof(ExtendedBooleanToVisibilityConverter));

        /// <summary>
        /// Gets or Sets a value indicating which <see cref="Visibility"/> to set when true is passed to the converter
        /// </summary>
        public Visibility TrueValue
        {
            get { return this.GetValueSafe<Visibility>(TrueValueProperty); }
            set { this.SetValueSafe(TrueValueProperty, value); }
        }

        /// <summary>
        /// The FalseValue property
        /// </summary>
        public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register("FalseValue", typeof(Visibility), typeof(ExtendedBooleanToVisibilityConverter));

        /// <summary>
        /// Gets or Sets a value indicating which <see cref="Visibility"/> to set when false is passed to the converter
        /// </summary>
        public Visibility FalseValue
        {
            get { return this.GetValueSafe<Visibility>(FalseValueProperty); }
            set { this.SetValueSafe(FalseValueProperty, value); }
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? val;
            if ((val = value as bool?) != null)
                return val == true ? TrueValue : FalseValue;
            else
                return Visibility.Visible;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                Visibility vis = (Visibility)value;
                return vis == TrueValue;
            }
            else
                return null;
        }
    }
}