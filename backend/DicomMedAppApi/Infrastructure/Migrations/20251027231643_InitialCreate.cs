using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DicomMedAppApi.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PatientName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "studies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudyInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StudyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StudyTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    StudyDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AccessionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReferringPhysicianName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_studies_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "series",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeriesInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Modality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SeriesNumber = table.Column<int>(type: "integer", nullable: true),
                    SeriesDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BodyPartExamined = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProtocolName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StudyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_series_studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SopInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SopClassUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    InstanceNumber = table.Column<int>(type: "integer", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    TransferSyntaxUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Rows = table.Column<int>(type: "integer", nullable: true),
                    Columns = table.Column<int>(type: "integer", nullable: true),
                    BitsAllocated = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_instances_series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_instances_SeriesId",
                table: "instances",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_instances_SopInstanceUid",
                table: "instances",
                column: "SopInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patients_PatientId",
                table: "patients",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_series_SeriesInstanceUid",
                table: "series",
                column: "SeriesInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_series_StudyId",
                table: "series",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_studies_PatientId",
                table: "studies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_studies_StudyInstanceUid",
                table: "studies",
                column: "StudyInstanceUid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "instances");

            migrationBuilder.DropTable(
                name: "series");

            migrationBuilder.DropTable(
                name: "studies");

            migrationBuilder.DropTable(
                name: "patients");
        }
    }
}
