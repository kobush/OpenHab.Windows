using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Coding4Fun.Toolkit.Controls;
using WinRTXamlToolkit.Controls.Extensions;

namespace OpenHab.UI.Services
{
    public interface IPromptService
    {
        void ShowNotification(string title, string message);
        void ShowError(string title, string message, IList<UICommand> commands);
    }

    public class PromptService : IPromptService
    {
        public void ShowNotification(string title, string message)
        {
            ToastPrompt toast = new ToastPrompt();
            toast.TextOrientation = Orientation.Vertical;
            toast.TextWrapping = TextWrapping.WrapWholeWords;
            toast.Title = title;
            toast.Message = message;
            toast.MillisecondsUntilHidden = 2000; // show for 3 seconds.

            //toast.ImageSource = new BitmapImage(new Uri("ApplicationIcon.png", UriKind.RelativeOrAbsolute));
            toast.Show();
        }

        public void ShowError(string title, string message, IList<UICommand> commands)
        {
            var dialog = new MessageDialog(message, title);
            if (commands != null)
                foreach (var cmd in commands)
                {
                    dialog.Commands.Add(cmd);
                }

            dialog.ShowAsyncIfPossible();
        }
    }
}
