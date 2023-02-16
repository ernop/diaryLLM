using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryUI.Migrations
{
    /// <inheritdoc />
    public partial class two : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transcripts_People_RecorderForeignKey",
                table: "Transcripts");

            migrationBuilder.AddColumn<string>(
                name: "HumanPrompt",
                table: "Queries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Transcripts_People_RecorderForeignKey",
                table: "Transcripts",
                column: "RecorderForeignKey",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transcripts_People_RecorderForeignKey",
                table: "Transcripts");

            migrationBuilder.DropColumn(
                name: "HumanPrompt",
                table: "Queries");

            migrationBuilder.AddForeignKey(
                name: "FK_Transcripts_People_RecorderForeignKey",
                table: "Transcripts",
                column: "RecorderForeignKey",
                principalTable: "People",
                principalColumn: "Id");
        }
    }
}
