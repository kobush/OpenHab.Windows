using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHab.UI.Services
{
    public class Settings
    {
        public bool UseDemoMode { get; set; }

        public string Hostname { get; set; }

        public string RemoteHostname { get; set; }

        public int? PortNumber { get; set; }

        public bool UseHttps { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Sitemap { get; set; }


        public Uri ResolveLocalUri()
        {
            if (string.IsNullOrEmpty(Hostname))
                return null;

            try
            {
                var scheme = UseHttps ? "https" : "http";
                UriBuilder builder;
                if (PortNumber.HasValue)
                    builder = new UriBuilder(scheme, Hostname, PortNumber.Value);
                else
                    builder = new UriBuilder(scheme, Hostname);

                return builder.Uri;
            }
            catch
            {
                return null;
            }
        }

        public Uri ResolveRemoteUrl()
        {
            if (string.IsNullOrEmpty(RemoteHostname))
                return null;

            try
            {
                var scheme = UseHttps ? "https" : "http";
                UriBuilder builder;
                if (PortNumber.HasValue)
                    builder = new UriBuilder(scheme, RemoteHostname, PortNumber.Value);
                else
                    builder = new UriBuilder(scheme, RemoteHostname);

                return builder.Uri;
            }
            catch
            {
                return null;
            }
        }

        public Uri ResolveIconUrl(string iconName)
        {
            return new Uri(ResolveLocalUri(), string.Format("/images/{0}.png", iconName));
        }

    }
}
