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

                    b.Property<string>("Label")
                        .HasColumnName("label")
                        .HasColumnType("text");

                    b.Property<string>("ManifestationIdentifier")
                        .HasColumnName("manifestation_identifier")
                        .HasColumnType("text");

                    b.Property<string>("PackageIdentifier")
                        .HasColumnName("package_identifier")
                        .HasColumnType("text");

                    b.Property<int>("PackageShortBNumber")
                        .HasColumnName("package_short_b_number")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Processed")
                        .HasColumnName("processed")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id")
                        .HasName("pk_manifestations");

                    b.ToTable("manifestations");
                });
#pragma warning restore 612, 618
        }
    }
}
