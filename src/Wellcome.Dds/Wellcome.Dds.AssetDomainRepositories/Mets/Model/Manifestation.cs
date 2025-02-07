﻿using System.Collections.Generic;
using System.Linq;
using Utils;
using Wellcome.Dds.AssetDomain.Mets;
using Wellcome.Dds.Common;

namespace Wellcome.Dds.AssetDomainRepositories.Mets.Model
{
    /// <summary>
    /// A bridge to IIIF. Updates the Sequence Physical File Ids with access conditions obtained from the sections
    /// </summary>
    public class Manifestation : BaseMetsResource, IManifestation
    {
        public List<IPhysicalFile> Sequence
        {
            get
            {
                LazyInit();
                return sequence;
            }
            set { sequence = value; }
        }

        /// <summary>
        /// Files that should be sent to the DLCS.
        /// One PhysicalFile might have more than one File pointer - e.g., Images have JP2 and ALTO
        /// but only the JP2 is synchronisable, or a video might have mpeg and transcript
        /// </summary>
        public List<IStoredFile> SynchronisableFiles
        {
            get
            {
                LazyInit();
                return synchronisableFiles;
            }
        }

        public IStructRange RootStructRange
        {
            get
            {
                LazyInit();
                return rootStructRange;
            }
            set { rootStructRange = value; }
        }

        
        public string FirstInternetType
        {
            get
            {
                LazyInit();
                return firstInternetType;
            }
        }

        public List<string> IgnoredStorageIdentifiers
        {
            get
            {
                LazyInit();
                return ignoredStorageIdentifiers;
            }
        }

        
            
        public string[] PermittedOperations
        {
            get
            {
                if (ParentModsData != null && Type == "PeriodicalIssue")
                {
                    if (ParentModsData.PlayerOptions > 0)
                    {
                        return LicensesAndOptions.Instance.GetPermittedOperations(
                            ParentModsData.PlayerOptions, FirstInternetType);
                    }
                }
                // ModsData will be null if Partial == true
                if (ModsData != null && ModsData.PlayerOptions > 0)
                {
                    return LicensesAndOptions.Instance.GetPermittedOperations(
                        ModsData.PlayerOptions, FirstInternetType);
                }
                if (ModsData != null && ModsData.DzLicenseCode.HasText())
                {
                    return LicensesAndOptions.Instance.GetPermittedOperations(
                        ModsData.DzLicenseCode, Type, FirstInternetType);
                }
                return new string[] {};
            }
        }


        public IStoredFile PosterImage
        {
            get
            {
                LazyInit();
                return posterImage;
            }
            set { posterImage = value; }
        }

        private Dictionary<string, IPhysicalFile> ByFileId { get; set; }
        private ILogicalStructDiv logicalStructDiv;
        private ILogicalStructDiv parentLogicalStructDiv;
        private IStoredFile posterImage;
        private bool initialised;
        private List<IPhysicalFile> sequence;
        // private List<IPhysicalFile> significantSequence;
        private List<IStoredFile> synchronisableFiles;
        private IStructRange rootStructRange;
        private List<string> ignoredStorageIdentifiers;
        private string firstInternetType;

        public Manifestation(ILogicalStructDiv structDiv, ILogicalStructDiv parentStructDiv = null)
        {
            logicalStructDiv = structDiv;
            Id = logicalStructDiv.ExternalId;
            ModsData = logicalStructDiv.GetMods();
            parentLogicalStructDiv = parentStructDiv;
            if (parentLogicalStructDiv != null)
            {
                ParentModsData = parentLogicalStructDiv.GetMods();
            }
            Label = GetLabel(logicalStructDiv, ModsData);
            Type = logicalStructDiv.Type;
            Order = logicalStructDiv.Order;
            SourceFile = structDiv.WorkStore.GetFileInfoForPath(structDiv.ContainingFileRelativePath); 
            if (logicalStructDiv.HasChildLink())
            {
                Partial = true;
            }
        }

        private void LazyInit()
        {
            if (!Partial && !initialised)
            {
                sequence = logicalStructDiv.GetPhysicalFiles();
                posterImage = logicalStructDiv.GetPosterImage();
                ByFileId = sequence.ToDictionary(pf => pf.Id);
                rootStructRange = BuildStructRange(logicalStructDiv);
                var ignoreAssetFilter = new IgnoreAssetFilter();
                if (sequence.HasItems())
                {                    
                    // When we want to include POSTER images in the DLCS sync operation, 
                    // we can add || f.Use == "POSTER" here. Wait till new DLCS before doing that.
                    synchronisableFiles = sequence.SelectMany(pf => pf.Files)
                        .Where(sf => 
                            sf.Use == "OBJECTS" ||  // Old workflows
                            sf.Use == "ACCESS" ||   // New workflows
                            sf.Use == "TRANSCRIPT")
                        .ToList();
                    var ignoredFiles = sequence.SelectMany(pf => pf.Files)
                        .Where(sf => 
                            sf.Use == "POSTER" || 
                            sf.Use == "ALTO" || 
                            sf.Use == "PRESERVATION")
                        .ToList();
                    
                    ignoredStorageIdentifiers = ignoredFiles.Select(sf => sf.StorageIdentifier).ToList();

                    var firstFile = sequence.FirstOrDefault();
                    if (firstFile != null)
                    {
                        firstInternetType = firstFile.MimeType.ToLowerInvariant().Trim();
                    }
                }
                else
                {
                    ignoredStorageIdentifiers = new List<string>(0);
                }
            }
            initialised = true;
        }

        private IStructRange BuildStructRange(ILogicalStructDiv div)
        {
            var mods = div.GetMods();
            var sr = new StructRange
            {
                Id = div.Id,
                Mods = mods,
                Label = GetLabel(div, mods),
                Type = div.Type,
                PhysicalFileIds = div.GetPhysicalFiles().Select(pf => pf.Id).ToList()
            };

            if (mods != null && mods.AccessCondition.HasText())
            {
                foreach (var fileId in sr.PhysicalFileIds)
                {
                    var file = ByFileId[fileId];
                    if (!file.AccessCondition.HasText())
                    {
                        file.AccessCondition = mods.AccessCondition;
                    }
                    else
                    {
                        // only replace if this one is more restrictive
                        var forCompare = new[] {file.AccessCondition, mods.AccessCondition};
                        file.AccessCondition = AccessCondition.GetMostSecureAccessCondition(forCompare);
                    }
                }
            }
            if (div.Children.HasItems())
            {
                sr.Children = div.Children.Select(BuildStructRange).ToList();
            }
            return sr;
        }
    }
}
