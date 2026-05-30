using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalSignature.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSignaturesAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                    table.CheckConstraint("ck_audit_events_immutable", "1=1");
                });

            migrationBuilder.CreateTable(
                name: "signatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignatureBytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificateThumbprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signatures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_DocumentId",
                table: "audit_events",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_UserId",
                table: "audit_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_signatures_DocumentId",
                table: "signatures",
                column: "DocumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "signatures");
        }
    }
}
