using IIIF.LegacyInclusions;
using Newtonsoft.Json;

namespace IIIF.Auth
{
    public class AuthLogoutService1: ServiceBase, IService
    {
        public AuthLogoutService1()
        {
            Profile = "http://iiif.io/api/auth/1/logout";
        }
        
        [JsonProperty(PropertyName = "@type", Order = 3)]
        public override string Type => nameof(AuthLogoutService1);
    }
}