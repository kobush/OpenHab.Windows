﻿using System;
using System.Collections.Generic;

namespace OpenHab.Client
{
    public class Item
    {
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public Uri Link { get; set; }

        public IList<Item> Members { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1}) = {2}", Name, Type, State);
        }
    }
}