using System;
using K8Cloud.Contracts.Kubernetes.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K8Cloud.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFullResourcesToNamespaceSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InSyncResouceTime",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.DropColumn(
                name: "SyncedResouceTime",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.AddColumn<NamespaceResource>(
                name: "InSyncResouce",
                table: "KubernetsNamespaceSyncState",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<NamespaceResource>(
                name: "SyncedResouce",
                table: "KubernetsNamespaceSyncState",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InSyncResouce",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.DropColumn(
                name: "SyncedResouce",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.AddColumn<DateTime>(
                name: "InSyncResouceTime",
                table: "KubernetsNamespaceSyncState",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedResouceTime",
                table: "KubernetsNamespaceSyncState",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
