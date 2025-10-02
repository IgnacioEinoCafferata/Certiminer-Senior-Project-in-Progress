using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certiminer.Data.Migrations
{
    public partial class AddTestIdToTestAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add TestId (nullable temporarily)
            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "TestAttempts",
                type: "int",
                nullable: true);

            // 2) If TestAttempts still has VideoId, copy to TestId via Videos.TestId
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TestAttempts','VideoId') IS NOT NULL
BEGIN
    UPDATE a
    SET a.TestId = v.TestId
    FROM dbo.TestAttempts a
    JOIN dbo.Videos v ON v.Id = a.VideoId;
END
");

            // 3) Fallback just in case (should be rare)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM dbo.TestAttempts WHERE TestId IS NULL)
BEGIN
    DECLARE @t INT = (SELECT TOP(1) Id FROM dbo.Tests ORDER BY Id);
    IF @t IS NOT NULL
        UPDATE dbo.TestAttempts SET TestId = @t WHERE TestId IS NULL;
END
");

            // 4) Create FK + index
            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_TestId",
                table: "TestAttempts",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestAttempts_Tests_TestId",
                table: "TestAttempts",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 5) Make TestId NOT NULL
            migrationBuilder.Sql(@"
UPDATE dbo.TestAttempts SET TestId = (SELECT TOP(1) Id FROM dbo.Tests ORDER BY Id)
WHERE TestId IS NULL;
");
            migrationBuilder.AlterColumn<int>(
                name: "TestId",
                table: "TestAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            // 6) Drop old VideoId (FK/IX/column) if present
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TestAttempts','VideoId') IS NOT NULL
BEGIN
    DECLARE @fk sysname =
    (SELECT TOP(1) fk.name
     FROM sys.foreign_keys fk
     WHERE fk.parent_object_id = OBJECT_ID(N'dbo.TestAttempts')
       AND fk.name LIKE 'FK%TestAttempts%VideoId%');
    IF @fk IS NOT NULL EXEC('ALTER TABLE dbo.TestAttempts DROP CONSTRAINT [' + @fk + ']');

    DECLARE @ix sysname =
    (SELECT TOP(1) i.name
     FROM sys.indexes i
     WHERE i.object_id = OBJECT_ID(N'dbo.TestAttempts')
       AND i.name LIKE 'IX%VideoId%');
    IF @ix IS NOT NULL EXEC('DROP INDEX [' + @ix + '] ON dbo.TestAttempts');

    ALTER TABLE dbo.TestAttempts DROP COLUMN VideoId;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate VideoId (nullable) for rollback
            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "TestAttempts",
                type: "int",
                nullable: true);

            // Drop FK/IX on TestId
            migrationBuilder.DropForeignKey(
                name: "FK_TestAttempts_Tests_TestId",
                table: "TestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_TestAttempts_TestId",
                table: "TestAttempts");

            // Drop TestId
            migrationBuilder.DropColumn(
                name: "TestId",
                table: "TestAttempts");
        }
    }
}
