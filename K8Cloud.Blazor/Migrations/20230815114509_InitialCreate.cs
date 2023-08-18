using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K8Cloud.Blazor.Migrations
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServerName = table.Column<string>(type: "TEXT", nullable: false),
                    ServerAddress = table.Column<string>(type: "TEXT", nullable: false),
                    ServerCertificateAuthorityData = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    UserCredentialsCertificateData = table.Column<string>(type: "TEXT", nullable: false),
                    UserCredentialsKeyData = table.Column<string>(type: "TEXT", nullable: false),
                    Namespace = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
