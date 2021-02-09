using Newtonsoft.Json;

namespace IIIF.LegacyInclusions
{
    public class SearchResultAnnotationResource : LegacyResourceBase
    {
        public override string Type => "cnt:ContentAsText";
        
        [JsonProperty(Order = 10, PropertyName = "chars")]
        public string? Chars { get; set; }
    }
}