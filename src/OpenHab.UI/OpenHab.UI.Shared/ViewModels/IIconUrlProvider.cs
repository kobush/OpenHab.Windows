using System;

namespace OpenHab.UI.ViewModels
{
    public interface IIconUrlProvider
    {
        Uri ResolveIconUrl(string icon);
    }

    //TODO: refactor to use settings 
    public class IconUrlProvider : IIconUrlProvider
    {
        public Uri ResolveIconUrl(string icon)
        {
            return new Uri(string.Format("http://192.168.1.2:8080/images/{0}.png", icon));
        }
    }
}