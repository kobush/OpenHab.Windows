using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OpenHab.UI.Helpers
{
    public class ContentDialogHelper : DependencyObject
    {
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.RegisterAttached(
            "IsOpen", typeof (bool), typeof (ContentDialogHelper),
                new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public static void SetIsOpen(DependencyObject d, bool value)
        {
            d.SetValue(IsOpenProperty, value);
        }

        public static bool GetIsOpen(DependencyObject d)
        {
            return (bool)d.GetValue(IsOpenProperty);
        }

        private static async void OnIsOpenPropertyChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            var dialog = sender as ContentDialog;
            if (dialog == null) return;

            var value = (bool) e.NewValue;
            if (value)
            {
                // subscribe to update state when dialog is closed
                dialog.Closed += OnDialogClosed;
                await dialog.ShowAsync();
            }
            else
            {
                dialog.Hide();
            }
        }

        private static void OnDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            sender.Closed -= OnDialogClosed;
            SetIsOpen(sender, false);
        }
    }
}
