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
                name: "Patients",
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
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Studies",
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
                    table.PrimaryKey("PK_Studies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Studies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Series",
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
                    table.PrimaryKey("PK_Series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Series_Studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
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
                    table.PrimaryKey("PK_Instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instances_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instances_SeriesId",
                table: "Instances",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Instances_SopInstanceUid",
                table: "Instances",
                column: "SopInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientId",
                table: "Patients",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_SeriesInstanceUid",
                table: "Series",
                column: "SeriesInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_StudyId",
                table: "Series",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_PatientId",
                table: "Studies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_StudyInstanceUid",
                table: "Studies",
                column: "StudyInstanceUid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Studies");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
