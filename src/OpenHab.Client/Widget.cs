using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenHab.Client
{
    public class Widget
    {
        public string WidgetId { get; set; }

        public WidgetType Type { get; set; }

        public string Label { get; set; }

        public string Icon { get; set; }

        [JsonProperty("widget")]
        [JsonConverter(typeof(SingleOrArrayConverter<Widget>))]
        public IList<Widget> Widgets { get; set; }

        public Item Item { get; set; }

        public Page LinkedPage { get; set; }
    }
}