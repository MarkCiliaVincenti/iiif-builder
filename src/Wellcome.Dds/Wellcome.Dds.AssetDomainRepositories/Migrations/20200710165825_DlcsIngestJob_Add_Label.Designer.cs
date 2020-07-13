﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Wellcome.Dds.AssetDomainRepositories;

namespace Wellcome.Dds.AssetDomainRepositories.Migrations
{
    [DbContext(typeof(DdsInstrumentationContext))]
    [Migration("20200710165825_DlcsIngestJob_Add_Label")]
    partial class DlcsIngestJob_Add_Label
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Wellcome.Dds.AssetDomain.Dlcs.Ingest.DlcsBatch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BatchSize")
                        .HasColumnName("batch_size")
                        .HasColumnType("integer");

                    b.Property<int?>("ContentLength")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("content_length")
                        .HasColumnType("integer");

                    b.Property<int>("DlcsIngestJobId")
                        .HasColumnName("dlcs_ingest_job_id")
                        .HasColumnType("integer");

                    b.Property<int>("ErrorCode")
                        .HasColumnName("error_code")
                        .HasColumnType("integer");

                    b.Property<string>("ErrorText")
                        .HasColumnName("error_text")
                        .HasColumnType("text");

                    b.Property<DateTime?>("Finished")
                        .HasColumnName("finished")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RequestBody")
                        .HasColumnName("request_body")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RequestSent")
                        .HasColumnName("request_sent")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ResponseBody")
                        .HasColumnName("response_body")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_dlcs_batches");

                    b.HasIndex("DlcsIngestJobId")
                        .HasName("ix_dlcs_batches_dlcs_ingest_job_id");

                    b.ToTable("dlcs_batches");
                });

            modelBuilder.Entity("Wellcome.Dds.AssetDomain.Dlcs.Ingest.DlcsIngestJob", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AssetType")
                        .HasColumnName("asset_type")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .HasColumnName("data")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EndProcessed")
                        .HasColumnName("end_processed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Identifier")
                        .HasColumnName("identifier")
                        .HasColumnType("text");

                    b.Property<int>("ImageCount")
                        .HasColumnName("image_count")
                        .HasColumnType("integer");

                    b.Property<string>("IssuePart")
                        .HasColumnName("issue_part")
                        .HasColumnType("text");

                    b.Property<string>("Label")
                        .HasColumnName("label")
                        .HasColumnType("text");

                    b.Property<int>("ReadyImageCount")
                        .HasColumnName("ready_image_count")
                        .HasColumnType("integer");

                    b.Property<int>("SequenceIndex")
                        .HasColumnName("sequence_index")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StartProcessed")
                        .HasColumnName("start_processed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Succeeded")
                        .HasColumnName("succeeded")
                        .HasColumnType("boolean");

                    b.Property<string>("VolumePart")
                        .HasColumnName("volume_part")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_dlcs_ingest_jobs");

                    b.ToTable("dlcs_ingest_jobs");
                });

            modelBuilder.Entity("Wellcome.Dds.AssetDomain.Dlcs.Ingest.IngestAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Action")
                        .HasColumnName("action")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnName("description")
                        .HasColumnType("text");

                    b.Property<int?>("JobId")
                        .HasColumnName("job_id")
                        .HasColumnType("integer");

                    b.Property<string>("ManifestationId")
                        .HasColumnName("manifestation_id")
                        .HasColumnType("text");

                    b.Property<DateTime>("Performed")
                        .HasColumnName("performed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Username")
                        .HasColumnName("username")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_ingest_actions");

                    b.ToTable("ingest_actions");
                });

            modelBuilder.Entity("Wellcome.Dds.AssetDomain.Workflow.WorkflowJob", b =>
                {
                    b.Property<string>("Identifier")
                        .HasColumnName("identifier")
                        .HasColumnType("text");

                    b.Property<int>("AnnosAlreadyOnDisk")
                        .HasColumnName("annos_already_on_disk")
                        .HasColumnType("integer");

                    b.Property<int>("AnnosBuilt")
                        .HasColumnName("annos_built")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("DlcsJobCount")
                        .HasColumnName("dlcs_job_count")
                        .HasColumnType("integer");

                    b.Property<string>("Error")
                        .HasColumnName("error")
                        .HasColumnType("text");

                    b.Property<int>("ExpectedTexts")
                        .HasColumnName("expected_texts")
                        .HasColumnType("integer");

                    b.Property<bool>("Finished")
                        .HasColumnName("finished")
                        .HasColumnType("boolean");

                    b.Property<int>("FirstDlcsJobId")
                        .HasColumnName("first_dlcs_job_id")
                        .HasColumnType("integer");

                    b.Property<bool>("ForceTextRebuild")
                        .HasColumnName("force_text_rebuild")
                        .HasColumnType("boolean");

                    b.Property<long>("PackageBuildTime")
                        .HasColumnName("package_build_time")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("Taken")
                        .HasColumnName("taken")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("TextAndAnnoBuildTime")
                        .HasColumnName("text_and_anno_build_time")
                        .HasColumnType("bigint");

                    b.Property<int>("TextPages")
                        .HasColumnName("text_pages")
                        .HasColumnType("integer");

                    b.Property<int>("TextsAlreadyOnDisk")
                        .HasColumnName("texts_already_on_disk")
                        .HasColumnType("integer");

                    b.Property<int>("TextsBuilt")
                        .HasColumnName("texts_built")
                        .HasColumnType("integer");

                    b.Property<int>("TimeSpentOnTextPages")
                        .HasColumnName("time_spent_on_text_pages")
                        .HasColumnType("integer");

                    b.Property<long>("TotalTime")
                        .HasColumnName("total_time")
                        .HasColumnType("bigint");

                    b.Property<bool>("Waiting")
                        .HasColumnName("waiting")
                        .HasColumnType("boolean");

                    b.Property<int>("Words")
                        .HasColumnName("words")
                        .HasColumnType("integer");

                    b.HasKey("Identifier")
                        .HasName("pk_workflow_jobs");

                    b.ToTable("workflow_jobs");
                });

            modelBuilder.Entity("Wellcome.Dds.AssetDomain.Dlcs.Ingest.DlcsBatch", b =>
                {
                    b.HasOne("Wellcome.Dds.AssetDomain.Dlcs.Ingest.DlcsIngestJob", null)
                        .WithMany("DlcsBatches")
                        .HasForeignKey("DlcsIngestJobId")
                        .HasConstraintName("fk_dlcs_batches_dlcs_ingest_jobs_dlcs_ingest_job_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
