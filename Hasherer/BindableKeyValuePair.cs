using System.Windows;
using T0yK4T.WPFTools;
using System;

namespace Hasherer
{
    /// <summary>
    /// Simple wpf-bindable KeyValuePair
    /// </summary>
    public class BindableKeyValuePair : DependencyObject
    {
        /// <summary>
        /// The Key property
        /// </summary>
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(object), typeof(BindableKeyValuePair));

        /// <summary>
        /// The Value property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(BindableKeyValuePair));

        public BindableKeyValuePair() { }
        public BindableKeyValuePair(object key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets or Sets the key of this <see cref="BindableKeyValuePair"/>
        /// </summary>
        public object Key
        {
            get { return this.GetValueSafe<object>(KeyProperty); }
            set { this.SetValueSafe(KeyProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the value of this <see cref="BindableKeyValuePair"/>
        /// </summary>
        public object Value
        {
            get { return this.GetValueSafe<object>(ValueProperty); }
            set { this.SetValueSafe(ValueProperty, value); }
        }
    }
}