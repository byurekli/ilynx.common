using System.Windows.Media.Animation;
using System.Windows;
using System;
namespace T0yK4T.WPFTools
{
    /// <summary>
    /// A simple grid length animation
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        /// <summary>
        /// The <see cref="From"/> property
        /// </summary>
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        /// <summary>
        /// The <see cref="To"/> property
        /// </summary>
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        /// <summary>
        /// The EasingFunction property
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation));

        private bool fromSet = false;

        /// <summary>
        /// Overridden to figure out which values are set
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == FromProperty)
                fromSet = true;
        }

        /// <summary>
        /// Gets or Sets a value indicating where the animation starts
        /// </summary>
        public GridLength From
        {
            get { return this.GetValueSafe<GridLength>(FromProperty); }
            set { this.SetValueSafe(FromProperty, value); }
        }

        /// <summary>
        /// Gets or Sets the <see cref="IEasingFunction"/> to use for this animation
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return this.GetValueSafe<IEasingFunction>(EasingFunctionProperty); }
            set { this.SetValue(EasingFunctionProperty, value); }
        }

        /// <summary>
        /// Gets or Sets a value indicating where the animation ends
        /// </summary>
        public GridLength To
        {
            get { return (GridLength)this.GetValueSafe<GridLength>(ToProperty); }
            set { this.SetValueSafe(ToProperty, value); }
        }

        /// <summary>
        /// <see cref="GridLength"/>
        /// </summary>
        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        /// <summary>
        /// <see cref="Freezable.CreateInstanceCore()"/>
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        /// <summary>
        /// </summary>
        /// <param name="defOrgVal"></param>
        /// <param name="defDstVal"></param>
        /// <param name="clock"></param>
        /// <returns></returns>
        public override object GetCurrentValue(object defOrgVal, object defDstVal, AnimationClock clock)
        {
            GridLength defOrg = (GridLength)defOrgVal;
            double from = fromSet ? this.From.Value : defOrg.Value, to = this.To.Value;
            double progress = clock.CurrentProgress.HasValue ? clock.CurrentProgress.Value : 1.0d;
            progress = this.EasingFunction != null ? this.EasingFunction.Ease(progress) : progress;
            if (from > to)
                return new GridLength((1 - progress) * (from - to) + to, GridUnitType.Star);
            else
                return new GridLength(progress * (to - from) + from, GridUnitType.Star);
        }
    }
}