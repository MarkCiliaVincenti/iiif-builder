﻿using IIIF;
using IIIF.Presentation.V2;
using IIIF.Presentation.V3.Strings;
using Newtonsoft.Json;
using Wellcome.Dds.Common;

namespace Wellcome.Dds.Repositories.Presentation.V2
{
    /// <summary>
    /// Service user to for tracking information in UV.
    /// </summary>
    public class TrackingExtensionsService : ResourceBase, IService
    {
        public override string? Type { get; set; } = null;
        
        [JsonProperty(Order = 5, PropertyName = "trackingLabel")]
        public string TrackingLabel { get; }

        public TrackingExtensionsService(DdsIdentifier identifier, LanguageMap trackingLabel)
        {
            TrackingLabel = trackingLabel.ToString();
            Id = $"http://wellcomelibrary.org/service/trackingLabels/{identifier}";
            Profile = Constants.Profiles.TrackingExtension;
            Context = "http://universalviewer.io/context.json";
        }
    }
}