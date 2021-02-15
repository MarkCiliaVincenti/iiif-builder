using IIIF.Presentation.V2;
using Newtonsoft.Json;

namespace IIIF.Auth.V1
{
    public class AuthTokenService : LegacyResourceBase, IService
    {
        public AuthTokenService()
        {
            Profile = "http://iiif.io/api/auth/1/token";
        }
        
        [JsonProperty(PropertyName = "@type", Order = 3)]
        public override string Type
        {
            get => nameof(AuthTokenService);
            set => throw new System.NotImplementedException();
        }
    }
}