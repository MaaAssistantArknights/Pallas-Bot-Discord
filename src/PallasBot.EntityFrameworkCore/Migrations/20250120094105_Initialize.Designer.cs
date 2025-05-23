﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PallasBot.EntityFrameworkCore;

#nullable disable

namespace PallasBot.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(PallasBotDbContext))]
    [Migration("20250120094105_Initialize")]
    partial class Initialize
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PallasBot.Domain.Entities.DiscordUserBinding", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discord_user_id");

                    b.Property<string>("GitHubLogin")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("github_login");

                    b.Property<decimal>("GitHubUserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("github_user_id");

                    b.HasKey("GuildId", "DiscordUserId");

                    b.ToTable("github_user_binding");
                });

            modelBuilder.Entity("PallasBot.Domain.Entities.DiscordUserRole", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.PrimitiveCollection<decimal[]>("RoleIds")
                        .IsRequired()
                        .HasColumnType("numeric(20,0)[]")
                        .HasColumnName("role_ids");

                    b.Property<DateTimeOffset>("UpdateAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("update_at");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("discord_user_role");
                });

            modelBuilder.Entity("PallasBot.Domain.Entities.DynamicConfiguration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Key")
                        .HasColumnType("text")
                        .HasColumnName("key");

                    b.Property<DateTimeOffset>("UpdateAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("update_at");

                    b.Property<decimal>("UpdateBy")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("update_by");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("GuildId", "Key");

                    b.ToTable("dynamic_configuration");
                });

            modelBuilder.Entity("PallasBot.Domain.Entities.GitHubContributor", b =>
                {
                    b.Property<string>("GitHubLogin")
                        .HasColumnType("text")
                        .HasColumnName("github_login");

                    b.Property<string>("Repository")
                        .HasColumnType("text")
                        .HasColumnName("repository");

                    b.HasKey("GitHubLogin", "Repository");

                    b.ToTable("github_contributors");
                });

            modelBuilder.Entity("PallasBot.Domain.Entities.GitHubOrganizationMember", b =>
                {
                    b.Property<string>("GitHubLogin")
                        .HasColumnType("text")
                        .HasColumnName("github_login");

                    b.HasKey("GitHubLogin");

                    b.ToTable("github_organization_member");
                });
#pragma warning restore 612, 618
        }
    }
}
