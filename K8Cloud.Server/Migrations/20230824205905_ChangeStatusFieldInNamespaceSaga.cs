using K8Cloud.Contracts.Kubernetes.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K8Cloud.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStatusFieldInNamespaceSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.AddColumn<Status>(
                name: "ErrorStatus",
                table: "KubernetsNamespaceSyncState",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorStatus",
                table: "KubernetsNamespaceSyncState");

            migrationBuilder.AddColumn<int>(
                name: "ErrorCode",
                table: "KubernetsNamespaceSyncState",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "KubernetsNamespaceSyncState",
                type: "text",
                nullable: true);
        }
    }
}
