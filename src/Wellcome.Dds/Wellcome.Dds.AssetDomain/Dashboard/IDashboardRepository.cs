﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Wellcome.Dds.AssetDomain.Dlcs;
using Wellcome.Dds.AssetDomain.Dlcs.Ingest;
using Wellcome.Dds.AssetDomain.Dlcs.Model;

namespace Wellcome.Dds.AssetDomain.Dashboard
{
    public interface IDashboardRepository
    {
        Task<IDigitisedResource> GetDigitisedResource(string identifier, bool includePdfDetails = false);
        Task<SyncOperation> GetDlcsSyncOperation(IDigitisedManifestation digitisedManifestation,
            bool reIngestErrorImages);
        Task ExecuteDlcsSyncOperation(SyncOperation syncOperation, bool usePriorityQueue);
        int DefaultSpace { get; }
        Task<IEnumerable<DlcsIngestJob>> GetMostRecentIngestJobs(string identifier, int number);
        //IEnumerable<DlcsIngestJob> GetUpdatedIngestJobs(string identifier, SyncOperation syncOperation);
        Task<Batch> GetBatch(string batchId);

        Task<JobActivity> GetRationalisedJobActivity(SyncOperation syncOperation);

        Task<IEnumerable<Batch>> GetBatchesForImages(IEnumerable<Image> images);
        Task<IEnumerable<ErrorByMetadata>> GetErrorsByMetadata();
        Task<Page<ErrorByMetadata>> GetErrorsByMetadata(int page);

        Task<int> FindSequenceIndex(string identifier);
        Task<bool> DeletePdf(string identifier);
        Task<int> RemoveOldJobs(string id);
        Task<int> DeleteOrphans(string id);
        
        IngestAction LogAction(string manifestationId, int? jobId, string userName, string action, string description = null);
        IEnumerable<IngestAction> GetRecentActions(int count, string user = null);
        Task<Dictionary<string, long>> GetDlcsQueueLevel();
        AVDerivative[] GetAVDerivatives(IDigitisedManifestation digitisedManifestation);
    }
}
