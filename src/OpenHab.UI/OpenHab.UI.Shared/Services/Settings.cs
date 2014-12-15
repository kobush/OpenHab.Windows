using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHab.UI.Services
{
    public class Settings
    {
        public string Hostname { get; set; }

        public int PortNumber { get; set; }

        public bool UseHttps { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Sitemap { get; set; }


        public Uri ResolveRestApiUri()
        {
            return null;
        }

        public Uri ResolveIconUri(string iconName)
        {
            return null;
        }
    }
}
