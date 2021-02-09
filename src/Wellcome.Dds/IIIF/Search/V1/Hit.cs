﻿using IIIF.LegacyInclusions;
using Newtonsoft.Json;

namespace IIIF.Search.V1
{
    public class Hit : LegacyResourceBase
    {
        public override string Type => "search:Hit";

        [JsonProperty(Order = 40, PropertyName = "annotations")]
        public string[] Annotations { get; set; }

        [JsonProperty(Order = 50, PropertyName = "match")]
        public string Match { get; set; }

        [JsonProperty(Order = 51, PropertyName = "before")]
        public string Before { get; set; }

        [JsonProperty(Order = 52, PropertyName = "after")]
        public string After { get; set; }

        // Not used for Wellcome
        [JsonProperty(Order = 60, PropertyName = "selectors")]

        public TextQuoteSelector[] Selectors { get; set; }
    }
}
