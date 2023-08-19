using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K8Cloud.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KubernetsClusters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerName = table.Column<string>(type: "text", nullable: false),
                    ServerAddress = table.Column<string>(type: "text", nullable: false),
                    ServerCertificateAuthorityData = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    UserCredentialsCertificateData = table.Column<string>(type: "text", nullable: false),
                    UserCredentialsKeyData = table.Column<string>(type: "text", nullable: false),
                    Namespace = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KubernetsClusters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KubernetsClusters_ServerName",
                table: "KubernetsClusters",
                column: "ServerName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KubernetsClusters");
        }
    }
}
