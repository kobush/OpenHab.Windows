using System;

namespace OpenHab.Client
{
    public class Sitemap
    {
        public string Name { get; set; }
        
        public string Label { get; set; }
        
        public Uri Link { get; set; }
        
        public SitemapPage Homepage { get; set; }
    }
}