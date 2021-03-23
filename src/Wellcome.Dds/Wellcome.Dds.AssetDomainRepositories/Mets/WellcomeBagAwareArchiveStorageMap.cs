﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Wellcome.Dds.AssetDomainRepositories.Mets
{
    /// <summary>
    /// Stores all individual files related to a bNumber, split by version.
    /// </summary>
    /// <remarks>See https://gist.github.com/tomcrane/441f82c635292c737323e98d6340f8df</remarks>
    [Serializable]
    public class WellcomeBagAwareArchiveStorageMap
    {
        /// <summary>
        /// a List of: "v1" => { "alto/#_0001.xml", ... }
        /// in increasing order of size of set 
        /// </summary>
        /// <remarks>This is a List, as opposed to Dict as ordering is important.</remarks>
        public List<KeyValuePair<string, HashSet<string>>> VersionSets { get; set; }
        public string BucketName { get; set; }
        public DateTime StorageManifestCreated { get; set; }
        public DateTime Built { get; set; }
        
        public string Identifier { get; set; }

        public static WellcomeBagAwareArchiveStorageMap FromJObject(JObject storageManifest, string identifier)
        {
            // This is the length of the substring "data/"
            const int dataPathElementOffset = 5;

            var accessLocation = storageManifest.SelectToken("location");
            var bucketName = accessLocation.Value<string>("bucket");
            var archiveStorageMap = new WellcomeBagAwareArchiveStorageMap
            {
                Identifier = identifier,
                BucketName = bucketName,
                StorageManifestCreated = storageManifest.Value<DateTime>("createdDate")
            };
            var pathSep = new[] {'/'};

            var versionToFiles = new Dictionary<string, HashSet<string>>();
            var manifest = storageManifest.SelectToken("manifest");
            foreach (var file in manifest["files"])
            {
                // strip "data/"
                // This makes an assumption that the file layout follows an expected structure
                // That's a valid assumption for the DDS to make, but not any other application using the storage
                var relativePath = file.Value<string>("name").Substring(dataPathElementOffset);
                var version = file.Value<string>("path").Split(pathSep).First();
                var minRelativePath = relativePath.Replace(identifier, "#");
                if (!versionToFiles.ContainsKey(version))
                {
                    versionToFiles[version] = new HashSet<string>();
                }

                versionToFiles[version].Add(minRelativePath);
            }
            
            // now order the dict by largest member
            archiveStorageMap.VersionSets = versionToFiles.OrderBy(kv => kv.Value.Count).ToList();
            
            archiveStorageMap.Built = DateTime.UtcNow;
            return archiveStorageMap;
        }
    }
}
