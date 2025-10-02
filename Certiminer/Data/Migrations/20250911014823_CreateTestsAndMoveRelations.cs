using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certiminer.Data.Migrations
{
    public partial class CreateTestsAndMoveRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Crear tabla Tests
            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            // 2) Agregar TestId a Videos y Questions (temporalmente nullable)
            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "Videos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestId",
                table: "Questions",
                type: "int",
                nullable: true);

            // 3) Migración de datos desde el modelo viejo (si existía Questions.VideoId)
            migrationBuilder.Sql(@"
-- 1) Crear Tests desde los Videos (UNO por título)
INSERT INTO dbo.Tests(Title, IsActive)
SELECT DISTINCT v.Title, 1
FROM dbo.Videos v
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Tests t WHERE t.Title = v.Title
);

-- 2) Asignar a cada Video su Test por Título
UPDATE v
SET v.TestId = t.Id
FROM dbo.Videos v
JOIN dbo.Tests t ON t.Title = v.Title;

-- 3) Si Questions todavía tiene VideoId, llevarlo a TestId usando el Video asignado
IF COL_LENGTH('dbo.Questions','VideoId') IS NOT NULL
BEGIN
    UPDATE q
    SET q.TestId = v.TestId
    FROM dbo.Questions q
    JOIN dbo.Videos v ON v.Id = q.VideoId;
END
");

            // 4) Asegurar que todas las Questions tengan TestId (por si quedó algo nulo)
            migrationBuilder.Sql(@"
IF EXISTS(SELECT 1 FROM dbo.Questions WHERE TestId IS NULL)
BEGIN
    DECLARE @fallbackTestId int = (SELECT TOP(1) Id FROM dbo.Tests ORDER BY Id);
    UPDATE dbo.Questions SET TestId = @fallbackTestId WHERE TestId IS NULL;
END
");

            // 5) Volver NOT NULL Questions.TestId
            migrationBuilder.AlterColumn<int>(
                name: "TestId",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            // 6) Índices y FKs nuevas
            migrationBuilder.CreateIndex(
                name: "IX_Videos_TestId",
                table: "Videos",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestId",
                table: "Questions",
                column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Tests_TestId",
                table: "Videos",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Tests_TestId",
                table: "Questions",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 7) Quitar columna vieja Questions.VideoId (primero FK/índice si existen)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Questions','VideoId') IS NOT NULL
BEGIN
    -- 1) Dropear FK si existe por nombre exacto
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Questions_Videos_VideoId'
    )
    BEGIN
        ALTER TABLE dbo.Questions DROP CONSTRAINT [FK_Questions_Videos_VideoId];
    END

    -- 2) Dropear índice si existe por nombre exacto
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Questions_VideoId'
          AND object_id = OBJECT_ID(N'dbo.Questions')
    )
    BEGIN
        DROP INDEX [IX_Questions_VideoId] ON dbo.Questions;
    END

    -- 3) Fallback por patrón (por si los nombres reales difieren)
    DECLARE @fk sysname =
    (
        SELECT TOP(1) fk.name
        FROM sys.foreign_keys fk
        WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Questions')
          AND fk.name LIKE 'FK%Questions%VideoId%'
    );
    IF @fk IS NOT NULL
        EXEC('ALTER TABLE dbo.Questions DROP CONSTRAINT [' + @fk + ']');

    DECLARE @ix sysname =
    (
        SELECT TOP(1) i.name
        FROM sys.indexes i
        WHERE i.object_id = OBJECT_ID(N'dbo.Questions')
          AND i.name LIKE 'IX%VideoId%'
    );
    IF @ix IS NOT NULL
        EXEC('DROP INDEX [' + @ix + '] ON dbo.Questions');

    -- 4) Ahora sí, borrar la columna
    ALTER TABLE dbo.Questions DROP COLUMN VideoId;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Volver a agregar VideoId (nullable) en Questions
            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "Questions",
                type: "int",
                nullable: true);

            // Quitar FKs/Índices nuevos
            migrationBuilder.DropForeignKey(name: "FK_Questions_Tests_TestId", table: "Questions");
            migrationBuilder.DropForeignKey(name: "FK_Videos_Tests_TestId", table: "Videos");

            migrationBuilder.DropIndex(name: "IX_Questions_TestId", table: "Questions");
            migrationBuilder.DropIndex(name: "IX_Videos_TestId", table: "Videos");

            // Quitar columnas TestId en tablas
            migrationBuilder.DropColumn(name: "TestId", table: "Questions");
            migrationBuilder.DropColumn(name: "TestId", table: "Videos");

            // Borrar tabla Tests
            migrationBuilder.DropTable(name: "Tests");
        }
    }
}
