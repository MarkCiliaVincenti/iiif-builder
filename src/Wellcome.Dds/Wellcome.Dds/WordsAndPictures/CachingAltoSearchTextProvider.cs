﻿using System;
using System.Threading.Tasks;
using Utils.Caching;
using Utils.Storage;

namespace Wellcome.Dds.WordsAndPictures
{
    public class CachingAltoSearchTextProvider : ISearchTextProvider
    {
        private readonly ISearchTextProvider altoSearchTextProvider;
        private readonly IBinaryObjectCache<Text> searchTextCache; 
        // needs options altoCache and httpRuntimeSeconds, prefix "alto_"
        
        public CachingAltoSearchTextProvider(
            ISearchTextProvider altoSearchTextProvider,
            IBinaryObjectCache<Text> searchTextCache)
        {
            this.altoSearchTextProvider = altoSearchTextProvider;
            this.searchTextCache = searchTextCache;
        }

        public Task<Text> GetSearchText(string identifier)
        {
            return GetSearchTextInternal(identifier, t => false);
        }

        public Task<Text> ForceSearchTextRebuild(string identifier)
        {
            return GetSearchTextInternal(identifier, t => true);
        }

        private async Task<Text> GetSearchTextInternal(string identifier, Predicate<Text> diskVersionIsStale)
        {
            Func<Task<Text>> getFromSource = () => altoSearchTextProvider.GetSearchText(identifier);
            return await searchTextCache.GetCachedObject(identifier, getFromSource, diskVersionIsStale);
        }


        public ISimpleStoredFileInfo GetFileInfo(string identifier)
        {
            return searchTextCache.GetCachedFile(identifier);
        }

        public void DeleteAltoCacheFile(string identifier)
        {
            searchTextCache.DeleteCacheFile(identifier);
        }
    }
}
