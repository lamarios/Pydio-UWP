using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Pydio.Models;

namespace Pydio.Migrations
{
    [DbContext(typeof(PydioContext))]
    [Migration("20160102173008_Migration1")]
    partial class Migration1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("Pydio.Models.Server", b =>
                {
                    b.Property<int>("ServerId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.Property<string>("Username");

                    b.HasKey("ServerId");
                });
        }
    }
}
