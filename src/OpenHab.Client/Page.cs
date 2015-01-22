using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenHab.Client
{
    public abstract class Page
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Icon { get; set; }

        public Uri Link { get; set; }

    }

    public class LinkedPage : Page
    {
    }

    public class SitemapPage : Page
    {
        [JsonProperty("widget")]
        [JsonConverter(typeof(SingleOrArrayConverter<Widget>))]
        public IList<Widget> Widgets { get; set; }

        public bool Leaf { get; set; }
    }
}