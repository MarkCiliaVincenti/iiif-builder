﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CatalogueClient.ToolSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utils;
using Wellcome.Dds.AssetDomain.Dashboard;
using Wellcome.Dds.AssetDomain.Workflow;
using Wellcome.Dds.AssetDomainRepositories;
using Wellcome.Dds.Catalogue;
using Wellcome.Dds.Common;

namespace WorkflowProcessor
{
    /// <summary>
    /// Background process that processes workflow_job records from database and creates dlcs_job record.
    /// </summary>
    public class WorkflowProcessorService : BackgroundService
    {
        private readonly ILogger<WorkflowProcessorService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly DdsOptions ddsOptions;
        private readonly string[] knownPopulationOperations = {"--populate-file", "--populate-slice"};
        private readonly string[] workflowOptionsParam = {"--workflow-options"};
        private readonly string FinishAllJobsParam = "--finish-all";
        private readonly string MopUpParam = "--mopup";
        private readonly string MopUpCDParam = "--mopupcd";
        private readonly string TraverseChemistAndDruggistParam = "--chem";
        private readonly string CatalogueDumpParam = "--catalogue-dump";
        private readonly string OffsetParam = "--offset";
        private readonly string ProcessParam = "--process";

        /// <summary>
        /// Usage:
        /// 
        /// (no args)
        /// Process workflow jobs from the table - standard continuous behaviour of this service.
        /// 
        /// --finish-all
        /// Mark all non-taken jobs as finished (reset them)
        /// 
        /// --populate-file {filepath}
        /// Create workflow jobs from the b numbers in a file
        /// 
        /// --populate-slice {skip}
        /// Create workflow jobs from a subset of all possible digitised b numbers.
        /// This will download and unpack the catalogue dump file, take every {skip} lines,
        /// produce a list of unique b numbers that have digital locations, then register jobs for them.
        /// e.g., skip 100 will populate 1% of the total possible jobs, skip 10 will populate 10%, skip 1 will do ALL jobs.
        ///
        /// --offset {offset}
        /// Create workflow jobs from a subset of all possible digitised b numbers.
        /// This will produce a list of b numbers that have digital locations, ignoring the first {offset} entries that
        /// have digitised b numbers (NOT skipping first X lines) 
        /// 
        /// --workflow-options {flags-int}
        /// Optional argument for the two populate-*** operations.
        /// This will create a job with a set of processing options that will override the default RunnerOptions, when
        /// the job is picked up by the WorkflowProcessor.
        /// This flags integer can be obtained by creating a new RunnerOptions instance and calling ToInt32().
        /// There is also a helper RunnerOptions.AllButDlcsSync() call for large-scale operations.
        /// 
        /// --workflow-options 30
        /// This is (currently) the all-but-DLCS flags value.
        /// 
        /// --workflow-options 6
        /// RefreshFlatManifestations and RebuildIIIF (no text or image registration)
        ///
        /// --mopup --workflow-options 6
        /// --mopupcd
        /// Reset all the special list of b numbers we use for testing, either with or without Chemist and Druggist
        ///
        /// --catalogue-dump {path}
        /// Specify the catalogue dump file to use, this will NOT download a fresh copy of catalogue
        ///
        /// --process {bnum}
        /// Creates workflow record and immediately processes specified bnumber
        /// 
        /// </summary>
        public WorkflowProcessorService(
            ILogger<WorkflowProcessorService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<DdsOptions> options)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.ddsOptions = options.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Verifying WorkflowProcessor configuration");
                using var scope = serviceScopeFactory.CreateScope();
                GetWorkflowRunner(scope);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to resolve WorkflowRunner object, aborting");
                throw;
            }
            
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("WorkflowProcessorService is shutting down");
            return base.StopAsync(cancellationToken);
        }

        private (string operation, string parameter) GetOperationWithParameter(string[] lookingFor)
        {
            var arguments = Environment.GetCommandLineArgs();
            logger.LogInformation("GetCommandLineArgs: {0}", string.Join(", ", arguments));
            return StringUtils.GetOperationAndParameter(arguments, lookingFor);
        }

        private bool HasArgument(string arg)
        {
            return Environment.GetCommandLineArgs().Contains(arg);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // if using launchSettings.json, you'll need to add something like
                // "commandLineArgs": "-- --populate-file /home/iiif-builder/src/Wellcome.Dds/WorkflowProcessor.Tests/examples.txt"
                // ...to run an examples file rather than default to job processing mode. 
                logger.LogInformation("Hosted service ExecuteAsync");
                var populationOperationWithParameter = GetOperationWithParameter(knownPopulationOperations);
                var workflowOptionsString = GetOperationWithParameter(workflowOptionsParam).parameter;
                int? workflowOptionsFlags = null;
                if (int.TryParse(workflowOptionsString, out var workflowOptionsValue))
                {
                    workflowOptionsFlags = workflowOptionsValue;
                }

                if (HasArgument(MopUpParam) || HasArgument(MopUpCDParam))
                {
                    logger.LogInformation($"Making workflow jobs for mop-up task");
                    await PopulateTextFixtures(HasArgument(MopUpCDParam), workflowOptionsFlags, stoppingToken);
                    return;
                }

                if (HasArgument(FinishAllJobsParam))
                {
                    int count = FinishAllJobs();
                    logger.LogInformation($"Force-finished {count} workflow jobs");
                    return;
                }

                if (HasArgument(TraverseChemistAndDruggistParam))
                {
                    logger.LogInformation("Print Chemist And Druggist Enumeration");
                    await TraverseChemistAndDruggist();
                    return;
                }

                if (HasArgument(ProcessParam))
                {
                    var processParam = GetOperationWithParameter(new[] {ProcessParam}).parameter;
                    logger.LogInformation($"Creating workflow record and processing {processParam}");
                    await ProcessBNumber(processParam, workflowOptionsFlags, stoppingToken);
                    return;
                }

                string? catalogueDump = null;
                if (HasArgument(CatalogueDumpParam))
                {
                    catalogueDump = GetOperationWithParameter(new[] {CatalogueDumpParam}).parameter;
                    logger.LogInformation("Using specified catalogue-dump file: {CatalogueDump}", catalogueDump);
                }

                int offset = 0;
                if (HasArgument(OffsetParam))
                {
                    var offsetString = GetOperationWithParameter(new[] {OffsetParam}).parameter;
                    if (int.TryParse(offsetString, out offset))
                    {
                        logger.LogInformation("Using offset = {Offset}", offset);
                    }
                }

                switch (populationOperationWithParameter.operation)
                {
                    // Population operations might be run from a desktop, against an RDS database.
                    // Or deployed to a temporary container and run from there.
                    case "--populate-file":
                        await PopulateJobsFromFile(populationOperationWithParameter.parameter, workflowOptionsFlags,
                            stoppingToken);
                        break;

                    case "--populate-slice":
                        await PopulateJobsFromSlice(populationOperationWithParameter.parameter, workflowOptionsFlags,
                            catalogueDump, offset, stoppingToken);
                        break;

                    default:
                        // This is what the workflowprocessor normally does! Takes jobs rather than creates them.
                        await PollForWorkflowJobs(stoppingToken);
                        break;
                }

                logger.LogInformation("Stopping WorkflowProcessorService...");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exception in WorkflowProcessor.ExecuteAsync");
            }
        }

        private async Task PopulateJobsFromSlice(string parameter, int? workflowOptions, string dumpFile, int offset,
            CancellationToken stoppingToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var workflowCallRepository = scope.ServiceProvider.GetRequiredService<IWorkflowCallRepository>();
            var catalogue = scope.ServiceProvider.GetRequiredService<ICatalogue>();
            if (!int.TryParse(parameter, out int skip))
            {
                throw new ArgumentException("Cannot parse skip integer", nameof(parameter));
            }

            var dumpLoopInfo = new DumpLoopInfo
            {
                Skip = skip,
                Offset = offset,
                Filter = DumpLoopInfo.IIIFLocationFilter
            };

            DumpUtils dumpUtils;
            if (!string.IsNullOrEmpty(dumpFile))
            {
                logger.LogInformation("Using existing dump file '{DumpFile}'", dumpFile);
                dumpUtils = new DumpUtils(dumpFile);
            }
            else
            {
                dumpUtils = new DumpUtils();
                logger.LogInformation("Downloading dump file (will take several minutes");
                await dumpUtils.DownloadDump();
                logger.LogInformation("Unpacking dump file");
                dumpUtils.UnpackDump();
            }

            dumpUtils.FindDigitisedBNumbers(dumpLoopInfo, catalogue);
            logger.LogInformation(
                $"{dumpLoopInfo.UniqueDigitisedBNumbers.Count} unique digitised b numbers found (from skip value of {skip})");
            int counter = 1;
            foreach (string uniqueDigitisedBNumber in dumpLoopInfo.UniqueDigitisedBNumbers)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Cancellation requested - aborting");
                    break;
                }

                await CreateNewWorkflowJob(workflowOptions, uniqueDigitisedBNumber, workflowCallRepository);
                logger.LogInformation($"({counter++} of {dumpLoopInfo.UniqueDigitisedBNumbers.Count})");
            }

            logger.LogInformation("Finished processing unique b numbers.");
        }

        private int FinishAllJobs()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var workflowCallRepository = scope.ServiceProvider.GetRequiredService<IWorkflowCallRepository>();
            return workflowCallRepository.FinishAllJobs();
        }
        
        private async Task PopulateTextFixtures(bool includeChemistAndDruggist, int? workflowOptions, CancellationToken stoppingToken)
        {
            var tests = new List<string>
            {
                "b2178081x",
                "b22454408",
                "b2043067x",
                "b24923333",
                "b30136155",
                "b16641097",
                "b16759230",
                "b16675630",
                "b28462270",
                "b17307922",
                "b29524404",
                "b20641151",
                "b24963215",
                "b24990796",
                "b29236927",
                "b20298341",
                "b19291449",
                "b19192162"
            };
            if (includeChemistAndDruggist)
            {
                tests.Add(KnownIdentifiers.ChemistAndDruggist);
            }
            await PopulateJobs(tests, workflowOptions, stoppingToken);
        }

        private async Task PopulateJobsFromFile(string file, int? workflowOptions, CancellationToken stoppingToken)
        {
            await PopulateJobs(File.ReadLines(file), workflowOptions, stoppingToken);
        }
        
        private async Task PopulateJobs(IEnumerable<string> bNumbers, int? workflowOptions, CancellationToken stoppingToken)
        {
            // https://stackoverflow.com/a/48368934
            using var scope = serviceScopeFactory.CreateScope();
            var workflowCallRepository = scope.ServiceProvider.GetRequiredService<IWorkflowCallRepository>();
            foreach (var bNumber in bNumbers)
            {
                if (bNumber.IsBNumber() && !stoppingToken.IsCancellationRequested)
                {
                    await CreateNewWorkflowJob(workflowOptions, bNumber, workflowCallRepository);
                }
                else
                {
                    logger.LogInformation($"Skipping line, '{bNumber}' is not a b number.");
                }
            }
        }

        private async Task CreateNewWorkflowJob(int? workflowOptions, string bNumber,
            IWorkflowCallRepository workflowCallRepository)
        {
            logger.LogInformation($"Attempting to create job for {bNumber}...");
            var job = await workflowCallRepository.CreateWorkflowJob(bNumber, workflowOptions);
            var displayString = "(none specified)";
            if (job.WorkflowOptions.HasValue)
            {
                displayString = RunnerOptions.FromInt32(job.WorkflowOptions.Value).ToDisplayString();
            }

            logger.LogInformation($"Job {job.Identifier} created with options {displayString}");
        }

        private async Task PollForWorkflowJobs(CancellationToken stoppingToken)
        {
            int waitMs = 2;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogDebug("Waiting for {wait} ms..", waitMs);
                    await Task.Delay(TimeSpan.FromMilliseconds(waitMs), stoppingToken);
                    
                    using var scope = serviceScopeFactory.CreateScope();

                    var statusProvider = scope.ServiceProvider.GetRequiredService<IStatusProvider>();
                    if (!await statusProvider.ShouldRunProcesses(stoppingToken))
                    {
                        logger.LogWarning("Status provider returned false, will not attempt to process");
                    }
                    else
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DdsInstrumentationContext>();

                        var jobId = dbContext.MarkFirstJobAsTaken(ddsOptions.MinimumJobAgeMinutes);
                        if (jobId == null)
                        {
                            waitMs = GetWaitMs(waitMs);
                            continue;
                        }

                        waitMs = 2;
                        var runner = GetWorkflowRunner(scope);
                        var job = await dbContext.WorkflowJobs.FindAsync(jobId);
                        await runner.ProcessJob(job, stoppingToken);
                        job.Finished = true;
                        try
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                        catch (DbUpdateException e)
                        {
                            throw new DdsInstrumentationDbException("Could not save workflow job: " + e.Message, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error running WorkflowProcessor");
                }
            }

            logger.LogInformation("Cancellation requested in WorkflowProcessor, shutting down.");
        }

        private async Task ProcessBNumber(string bnumber, int? workflowOptions, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DdsInstrumentationContext>();
                
                var workflowJob = await dbContext.PutJob(bnumber, true, true, workflowOptions, false, false);
                
                var runner = GetWorkflowRunner(scope);
                await runner.ProcessJob(workflowJob, stoppingToken);
                
                workflowJob.Finished = true;
                try
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (DbUpdateException e)
                {
                    throw new DdsInstrumentationDbException("Could not save workflow job: " + e.Message, e);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Processing BNumber {BNumber}", bnumber);
            }
        }

        private async Task TraverseChemistAndDruggist()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var runner = GetWorkflowRunner(scope);
            await runner.TraverseChemistAndDruggist();
        }

        private static WorkflowRunner GetWorkflowRunner(IServiceScope scope) =>
            scope.ServiceProvider.GetRequiredService<WorkflowRunner>();

        private static int GetWaitMs(int waitMs)
        {
            waitMs *= 2;

            // don't wait more than 5 mins
            if (waitMs > 300000)
            {
                waitMs = 300000;
            }

            return waitMs;
        }
    }
}