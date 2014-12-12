using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenHab.Client
{
    public class Page
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public Uri Link { get; set; }

        public bool Leaf { get; set; }

        [JsonProperty("widget")]
        [JsonConverter(typeof(SingleOrArrayConverter<Widget>))]
        public IList<Widget> Widgets { get; set; }
    }
}