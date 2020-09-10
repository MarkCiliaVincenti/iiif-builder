﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Wellcome.Dds.Repositories;

namespace Wellcome.Dds.Repositories.Migrations
{
    [DbContext(typeof(DdsContext))]
    partial class DdsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Wellcome.Dds.Manifestation", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<string>("AssetType")
                        .HasColumnName("asset_type")
                        .HasColumnType("text");

                    b.Property<string>("CalmAltRef")
                        .HasColumnName("calm_alt_ref")
                        .HasColumnType("text");

                    b.Property<string>("CalmAltRefParent")
                        .HasColumnName("calm_alt_ref_parent")
                        .HasColumnType("text");

                    b.Property<string>("CalmRef")
                        .HasColumnName("calm_ref")
                        .HasColumnType("text");

                    b.Property<string>("CalmRefParent")
                        .HasColumnName("calm_ref_parent")
                        .HasColumnType("text");

                    b.Property<string>("CatalogueThumbnail")
                        .HasColumnName("catalogue_thumbnail")
                        .HasColumnType("text");

                    b.Property<string>("CatalogueThumbnailDimensions")
                        .HasColumnName("catalogue_thumbnail_dimensions")
                        .HasColumnType("text");

                    b.Property<string>("CollectionReferenceNumber")
                        .HasColumnName("collection_reference_number")
                        .HasColumnType("text");

                    b.Property<string>("DipStatus")
                        .HasColumnName("dip_status")
                        .HasColumnType("text");

                    b.Property<string>("DlcsAssetType")
                        .HasColumnName("dlcs_asset_type")
                        .HasColumnType("text");

                    b.Property<int>("FileCount")
                        .HasColumnName("file_count")
                        .HasColumnType("integer");

                    b.Property<string>("FirstFileExtension")
                        .HasColumnName("first_file_extension")
                        .HasColumnType("text");

                    b.Property<string>("FirstFileStorageIdentifier")
                        .HasColumnName("first_file_storage_identifier")
                        .HasColumnType("text");

                    b.Property<string>("FirstFileThumbnail")
                        .HasColumnName("first_file_thumbnail")
                        .HasColumnType("text");

                    b.Property<string>("FirstFileThumbnailDimensions")
                        .HasColumnName("first_file_thumbnail_dimensions")
                        .HasColumnType("text");

                    b.Property<int>("Index")
                        .HasColumnName("index")
                        .HasColumnType("integer");

                    b.Property<bool>("IsAllOpen")
                        .HasColumnName("is_all_open")
                        .HasColumnType("boolean");

                    b.Property<string>("Label")
                        .HasColumnName("label")
                        .HasColumnType("text");

                    b.Property<string>("ManifestationFile")
                        .HasColumnName("manifestation_file")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ManifestationFileModified")
                        .HasColumnName("manifestation_file_modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ManifestationIdentifier")
                        .HasColumnName("manifestation_identifier")
                        .HasColumnType("text");

                    b.Property<string>("PackageFile")
                        .HasColumnName("package_file")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PackageFileModified")
                        .HasColumnName("package_file_modified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("PackageIdentifier")
                        .HasColumnName("package_identifier")
                        .HasColumnType("text");

                    b.Property<string>("PackageLabel")
                        .HasColumnName("package_label")
                        .HasColumnType("text");

                    b.Property<int>("PackageShortBNumber")
                        .HasColumnName("package_short_b_number")
                        .HasColumnType("integer");

                    b.Property<string>("PermittedOperations")
                        .HasColumnName("permitted_operations")
                        .HasColumnType("text");

                    b.Property<DateTime>("Processed")
                        .HasColumnName("processed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ReferenceNumber")
                        .HasColumnName("reference_number")
                        .HasColumnType("text");

                    b.Property<string>("RootSectionAccessCondition")
                        .HasColumnName("root_section_access_condition")
                        .HasColumnType("text");

                    b.Property<string>("RootSectionType")
                        .HasColumnName("root_section_type")
                        .HasColumnType("text");

                    b.Property<bool>("SupportsSearch")
                        .HasColumnName("supports_search")
                        .HasColumnType("boolean");

                    b.Property<string>("VolumeIdentifier")
                        .HasColumnName("volume_identifier")
                        .HasColumnType("text");

                    b.Property<string>("WorkId")
                        .HasColumnName("work_id")
                        .HasColumnType("text");

                    b.Property<string>("WorkType")
                        .HasColumnName("work_type")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_manifestations");

                    b.ToTable("manifestations");
                });

            modelBuilder.Entity("Wellcome.Dds.Metadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Identifier")
                        .HasColumnName("identifier")
                        .HasColumnType("text");

                    b.Property<string>("Label")
                        .HasColumnName("label")
                        .HasColumnType("text");

                    b.Property<string>("ManifestationId")
                        .HasColumnName("manifestation_id")
                        .HasColumnType("text");

                    b.Property<string>("StringValue")
                        .HasColumnName("string_value")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_metadata");

                    b.ToTable("metadata");
                });
#pragma warning restore 612, 618
        }
    }
}
