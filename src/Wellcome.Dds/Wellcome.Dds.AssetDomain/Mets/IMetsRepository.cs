﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wellcome.Dds.AssetDomain.Mets
{
    public interface IMetsRepository
    {
        Task<IMetsResource> GetAsync(string identifier);

        IAsyncEnumerable<IManifestationInContext> GetAllManifestationsInContextAsync(string identifier);
         
        int FindSequenceIndex(string identifier);
    }
}
