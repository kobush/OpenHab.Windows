using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;

namespace OpenHab.UI.Helpers
{
    public class DeferredSliderUpdateBehavior : DependencyObject, IBehavior
    {
        private Slider _slider;
        private DispatcherTimer _timer;

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof (ICommand), typeof (DeferredSliderUpdateBehavior), 
                new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(
            "Delay", typeof (TimeSpan), typeof (DeferredSliderUpdateBehavior), 
                new PropertyMetadata(TimeSpan.FromMilliseconds(100)));

        public TimeSpan Delay
        {
            get { return (TimeSpan) GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        public DependencyObject AssociatedObject
        {
            get { return _slider; }
        }

        public void Attach(DependencyObject associatedObject)
        {
            _slider = associatedObject as Slider;
            if (_slider != null)
                _slider.ValueChanged += OnSliderValueChanged;

        }

        public void Detach()
        {
            if (_slider != null)
                _slider.ValueChanged -= OnSliderValueChanged;
        }

        private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += Timer_Tick;
            }

            _timer.Interval = Delay;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();

            if (Command != null)
                Command.Execute(_slider.Value);
        }
    }
}