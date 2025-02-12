﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utils;
using Utils.Aws.S3;
using Utils.Caching;
using Wellcome.Dds.AssetDomain;
using Wellcome.Dds.AssetDomainRepositories.Mets;
using Wellcome.Dds.Common;

namespace Wellcome.Dds.AssetDomainRepositories.Storage.WellcomeStorageService
{
    /// <summary>
    /// Implementation of <see cref="IWorkStorageFactory"/> for works from Wellcome storage service.
    /// </summary>
    public class ArchiveStorageServiceWorkStorageFactory : IWorkStorageFactory
    {
        private readonly StorageServiceClient storageServiceClient;
        private readonly IAmazonS3 storageServiceS3;
        private readonly ILogger<ArchiveStorageServiceWorkStorageFactory> logger;
        private readonly StorageOptions storageOptions;
        private readonly IBinaryObjectCache<WellcomeBagAwareArchiveStorageMap> storageMapCache;
        private readonly Dictionary<string, XElement> xmlElementCache = new();

        public ArchiveStorageServiceWorkStorageFactory(
            ILogger<ArchiveStorageServiceWorkStorageFactory> logger,
            IOptions<StorageOptions> storageOptions,
            IBinaryObjectCache<WellcomeBagAwareArchiveStorageMap> storageMapCache,
            INamedAmazonS3ClientFactory storageServiceS3,
            StorageServiceClient storageServiceClient)
        {
            this.logger = logger;
            this.storageOptions = storageOptions.Value;
            this.storageMapCache = storageMapCache;
            this.storageServiceClient = storageServiceClient;
            this.storageServiceS3 = storageServiceS3.Get(NamedClient.Storage);
        }

        public async Task<IWorkStore> GetWorkStore(string identifier)
        {
            var ddsId = new DdsIdentifier(identifier);

            Func<Task<WellcomeBagAwareArchiveStorageMap>> getFromSource = () => 
                BuildStorageMap(ddsId.StorageSpace, ddsId.PackageIdentifier);
            
            logger.LogInformation("Getting IWorkStore for {Identifier}", ddsId.PackageIdentifier);
            
            WellcomeBagAwareArchiveStorageMap storageMap =
                await storageMapCache.GetCachedObject(ddsId.PackageIdentifier, getFromSource, NeedsRebuilding);
            
            return new ArchiveStorageServiceWorkStore(
                ddsId.StorageSpace, ddsId.PackageIdentifier,
                storageMap, storageServiceClient, xmlElementCache, storageServiceS3);
        }

        private async Task<WellcomeBagAwareArchiveStorageMap> BuildStorageMap(string storageSpace, string packageIdentifier)
        {
            logger.LogInformation("Requires new build of storage map for {Identifier}", packageIdentifier);
            var storageManifest = await storageServiceClient.LoadStorageManifest(storageSpace, packageIdentifier);
            var wellcomeBagAwareArchiveStorageMap = WellcomeBagAwareArchiveStorageMap.FromJObject(storageManifest, packageIdentifier);
            if (wellcomeBagAwareArchiveStorageMap.VersionSets.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"Unable to generate VersionSet for StorageMap. Id: {packageIdentifier}");
            }
            return wellcomeBagAwareArchiveStorageMap;
        }
        
        private bool NeedsRebuilding(WellcomeBagAwareArchiveStorageMap map)
        {
            if (map.VersionSets.IsNullOrEmpty())
            {
                logger.LogWarning("Cached StorageMap found with null or empty VersionSet. {Identifier}",
                    map.Identifier);
                return true;
            }
            
            if (storageOptions.PreferCachedStorageMap)
            {
                return false;
            }

            if (storageOptions.MaxAgeStorageMap < 0)
            {
                return false;
            }

            if (map.Identifier == KnownIdentifiers.ChemistAndDruggist)
            {
                // Never rebuild Chemist and Druggist's storage map on demand
                return false;
            }

            var age = DateTime.UtcNow - map.Built;
            return age.TotalSeconds > storageOptions.MaxAgeStorageMap;
        }
    }
}
