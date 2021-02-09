using IIIF.Presentation.Content;

namespace IIIF.Search.V2
{
    public class SearchService2 : ExternalResource, IService
    {
        public const string Search2Context = "http://iiif.io/api/search/2/context.json";
        public const string Search2Profile = "http://iiif.io/api/search/2/search";
        
        public SearchService2() : base(nameof(SearchService2))
        {
            // Allow callers to decide whether to set the @context
            Profile = Search2Profile;
        }
        
    }
}