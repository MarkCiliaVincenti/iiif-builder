﻿using Amazon.S3;
using DlcsWebClient.Dlcs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuth2;
using Utils.Aws.S3;
using Utils.Caching;
using Utils.Storage;
using Wellcome.Dds;
using Wellcome.Dds.AssetDomain;
using Wellcome.Dds.AssetDomain.Dashboard;
using Wellcome.Dds.AssetDomain.Dlcs.Ingest;
using Wellcome.Dds.AssetDomain.Mets;
using Wellcome.Dds.AssetDomainRepositories;
using Wellcome.Dds.AssetDomainRepositories.Dashboard;
using Wellcome.Dds.AssetDomainRepositories.Ingest;
using Wellcome.Dds.AssetDomainRepositories.Mets;
using Wellcome.Dds.Catalogue;
using Wellcome.Dds.Common;
using Wellcome.Dds.IIIFBuilding;
using Wellcome.Dds.Repositories.Catalogue;
using Wellcome.Dds.Repositories.Presentation;
using Wellcome.Dds.Repositories.WordsAndPictures;
using Wellcome.Dds.WordsAndPictures;

namespace WorkflowProcessor
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DdsInstrumentationContext>(options => options
                .UseNpgsql(Configuration.GetConnectionString("DdsInstrumentation"))
                .UseSnakeCaseNamingConvention());

            services.Configure<DdsOptions>(Configuration.GetSection("Dds"));
            services.Configure<RunnerOptions>(Configuration.GetSection("WorkflowProcessor"));
            services.Configure<StorageOptions>(Configuration.GetSection("Storage"));
            services.Configure<BinaryObjectCacheOptionsByType>(Configuration.GetSection("BinaryObjectCache"));
            
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions("Dds-AWS"));            
            var factory = services.AddNamedS3Clients(Configuration, NamedClient.All);

            services.AddSingleton(typeof(IBinaryObjectCache<>), typeof(BinaryObjectCache<>));
            services.AddSingleton<ISimpleCache, ConcurrentSimpleMemoryCache>();

            services.AddSingleton<IStorage, S3Storage>(opts =>
                ActivatorUtilities.CreateInstance<S3Storage>(opts, 
                    factory.Get(NamedClient.Dds)));
            services.AddScoped<IIIIFBuilder, IIIFBuilder>(opts =>
                ActivatorUtilities.CreateInstance<IIIFBuilder>(opts,
                    factory.Get(NamedClient.Dds)));
            services.AddScoped<WorkflowRunner>(opts =>
                ActivatorUtilities.CreateInstance<WorkflowRunner>(opts,
                    factory.Get(NamedClient.Dds)));

            services.AddMemoryCache();

            services.AddHttpClient<OAuth2ApiConsumer>();
            services.AddDlcsClient(Configuration);
            services.AddHttpClient<ICatalogue, WellcomeCollectionCatalogue>();
            services
                .AddScoped<IMetsRepository, MetsRepository>()
                .AddScoped<IIngestJobRegistry, CloudServicesIngestRegistry>()
                .AddScoped<IDashboardRepository, DashboardRepository>()
                .AddScoped<IWorkStorageFactory, ArchiveStorageServiceWorkStorageFactory>()
                .AddScoped<StorageServiceClient>()
                .AddScoped<Synchroniser>()
                .AddScoped<ISearchTextProvider, AltoSearchTextProvider>()
                .AddScoped<CachingAllAnnotationProvider>()
                .AddScoped<CachingAltoSearchTextProvider>()
                .AddSingleton<ISimpleCache, ConcurrentSimpleMemoryCache>()
                .AddHostedService<WorkflowProcessorService>();
            
            services.AddHealthChecks()
                .AddDbContextCheck<DdsInstrumentationContext>("DdsInstrumentation-db");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapHealthChecks("/management/healthcheck"));
        }
    }
}