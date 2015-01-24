using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace OpenHab.UI.Helpers
{
    //http://marcominerva.wordpress.com/2014/09/11/behaviors-to-handle-statusbar-and-progressindicator-in-windows-phone-8-1-apps/
    //http://www.visuallylocated.com/post/2014/04/06/Using-a-behavior-to-control-the-ProgressIndicator-in-Windows-Phone-81-XAML-Apps.aspx
    public class StatusBarBehavior : DependencyObject, IBehavior
    {
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible",
            typeof(bool),
            typeof(StatusBarBehavior),
            new PropertyMetadata(true, OnIsVisibleChanged));


        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isvisible = (bool)e.NewValue;
            if (isvisible)
            {
                StatusBar.GetForCurrentView().ShowAsync();
            }
            else
            {
                StatusBar.GetForCurrentView().HideAsync();
            }
        }

        public double BackgroundOpacity
        {

            get { return (double)GetValue(BackgroundOpacityProperty); }

            set { SetValue(BackgroundOpacityProperty, value); }

        }



        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register("BackgroundOpacity",
            typeof(double),
            typeof(StatusBarBehavior),
            new PropertyMetadata(0d, OnOpacityChanged));

        private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusBar.GetForCurrentView().BackgroundOpacity = (double)e.NewValue;
        }

        public Color? ForegroundColor
        {
            get { return (Color?)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor",
            typeof(Color),
            typeof(StatusBarBehavior),
            new PropertyMetadata(null, OnForegroundColorChanged));

        private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusBar.GetForCurrentView().ForegroundColor = (Color)e.NewValue;
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor",
            typeof(Color),
            typeof(StatusBarBehavior),
            new PropertyMetadata(null, OnBackgroundChanged));


        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (StatusBarBehavior)d;
            StatusBar.GetForCurrentView().BackgroundColor = behavior.BackgroundColor;

            // if they have not set the opacity, we need to so the new color is shown
            if (behavior.BackgroundOpacity == 0)
            {
                behavior.BackgroundOpacity = 1;
            }
        }

        public void Attach(DependencyObject associatedObject)
        { }

        public void Detach()
        { }

        public DependencyObject AssociatedObject { get; private set; }

    }

    public class ProgressBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
            typeof(string),
            typeof(ProgressBehavior),
                new PropertyMetadata(null, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBehavior behavior = (ProgressBehavior)d;
            StatusBar.GetForCurrentView().ProgressIndicator.Text = behavior.Text;
        }


        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible",
            typeof(bool),
            typeof(ProgressBehavior),
            new PropertyMetadata(false, OnIsVisibleChanged));

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }


        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBehavior behavior = (ProgressBehavior)d;

            bool isvisible = (bool)e.NewValue;
            if (isvisible)
            {
                StatusBar.GetForCurrentView().ProgressIndicator.Text = behavior.Text;
                StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
        }


        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
            typeof(object),
            typeof(ProgressBehavior),
            new PropertyMetadata(null, OnValueChanged));

        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double? val = (double?)e.NewValue;
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = val;
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        { }

        public void Detach()
        { }

    }
}